using DummyProject.Lib;

namespace DummyProject
{
    class Component
    {
        private readonly IBuilder myBuilder;

        public Component(IBuilder builder)
        {
            myBuilder = builder;
        }

        public void Init()
        {
            myBuilder.Build();
        }
    }
}
