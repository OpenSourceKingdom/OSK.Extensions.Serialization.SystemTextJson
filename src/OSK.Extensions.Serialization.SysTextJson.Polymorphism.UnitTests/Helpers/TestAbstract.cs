using OSK.Serialization.Polymorphism.Discriminators;

namespace OSK.Extensions.Serialization.SysTextJson.Polymorphism.UnitTests.Helpers
{
    [Discriminator(nameof(AbstractType), classTemplate: "Test{0}")]
    public abstract class TestAbstract
    {
        public int A { get; set; }

        public List<int> B { get; set; }

        public TestChildB C { get; set; }

        public AbstractType AbstractType { get; set; }
    }
}
