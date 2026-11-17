using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;

namespace Plainion.GraphViz.Actors.Client;

public class ActorClientBase : IDisposable
{
    private ActorSystem mySystem;
    private Process myHost;
    private Address myRemoteAddress;
    private readonly SemaphoreSlim myStartupLock = new(1, 1);

    public bool HideHostWindow { get; set; }

    protected async Task<object> ProcessAsync<TRequest>(Type actorType, TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await StartActorSystemOnDemandAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return null;
        }

        var system = mySystem;
        var host = myHost;
        var actor = system.ActorOf(Props.Create(actorType)
            .WithDeploy(Deploy.None.WithScope(new RemoteScope(myRemoteAddress))));

        using var lifetime = new CancellationTokenSource();

        try
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            var responseTask = actor.Ask<object>(request, cancellationToken: lifetime.Token);
            using var registration = cancellationToken.Register(() => actor.Tell(new CancelMessage()));
            var hostExit = host.WaitForExitAsync(lifetime.Token);
            if (await Task.WhenAny(responseTask, hostExit) == hostExit)
            {
                throw new InvalidOperationException($"Actor host exited with code {host.ExitCode} before responding.");
            }

            var response = await responseTask;
            if (response is FailedMessage m)
            {
                throw new Exception(m.Error);
            }
            else if (response is CanceledMessage)
            {
                return null;
            }
            else
            {
                return response;
            }
        }
        finally
        {
            lifetime.Cancel();
            system.Stop(actor);
        }
    }

    private async Task StartActorSystemOnDemandAsync(CancellationToken cancellationToken)
    {
        await myStartupLock.WaitAsync(cancellationToken);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (mySystem != null && !myHost.HasExited)
            {
                return;
            }

            ShutdownActorSystem();

            var pipeName = $"Plainion.GraphViz.{Guid.NewGuid():N}";
            using var pipe = new NamedPipeServerStream(pipeName, PipeDirection.In, 1,
                PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
            using var startup = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            startup.CancelAfter(TimeSpan.FromSeconds(30));

            var executable = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "Plainion.GraphViz.Actors.Host.exe");
            var info = new ProcessStartInfo(executable)
            {
                UseShellExecute = false,
                CreateNoWindow = HideHostWindow
            };
            info.ArgumentList.Add(pipeName);

            try
            {
                startup.Token.ThrowIfCancellationRequested();
                myHost = Process.Start(info);

                var connection = pipe.WaitForConnectionAsync(startup.Token);
                var hostExit = myHost.WaitForExitAsync(startup.Token);
                if (await Task.WhenAny(connection, hostExit) == hostExit && myHost.HasExited)
                {
                    throw new InvalidOperationException($"Actor host exited with code {myHost.ExitCode} during startup.");
                }

                await connection;
                using var reader = new StreamReader(pipe);
                myRemoteAddress = Address.Parse(await reader.ReadLineAsync(startup.Token));

                var config = ConfigurationFactory.ParseString(@"
                akka {
                    actor {
                        provider = remote
                    }
                    remote {
                        dot-netty.tcp {
                            port = 0
                            hostname = localhost
                            maximum-frame-size = 4000000b
                        }                   
                    }
                }");

                mySystem = ActorSystem.Create("CodeInspectionClient", config);
            }
            catch (OperationCanceledException ex) when (startup.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                ShutdownActorSystem();
                throw new TimeoutException("Actor host did not become ready within 30 seconds.", ex);
            }
            catch
            {
                ShutdownActorSystem();
                throw;
            }
            finally
            {
                startup.Cancel();
            }
        }
        finally
        {
            myStartupLock.Release();
        }
    }

    private void ShutdownActorSystem()
    {
        try
        {
            mySystem?.Dispose();
        }
        finally
        {
            mySystem = null;

            if (myHost != null)
            {
                try
                {
                    if (!myHost.HasExited)
                    {
                        myHost.Kill();
                        myHost.WaitForExit();
                    }
                }
                finally
                {
                    myHost.Dispose();
                    myHost = null;
                }
            }
        }
    }

    public void Dispose()
    {
        ShutdownActorSystem();
    }
}
