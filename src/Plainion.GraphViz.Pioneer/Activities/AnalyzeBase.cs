using Plainion.GraphViz.Pioneer.Spec;

namespace Plainion.GraphViz.Pioneer.Activities
{
    abstract class AnalyzeBase
    {
        protected Config Config { get; private set; }

        public void Execute(Config config)
        {
            Config = config;

            Execute();
        }

        protected abstract void Execute();
    }
}
