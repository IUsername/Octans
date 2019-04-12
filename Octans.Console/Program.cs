namespace Octans.ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //CornellBox.TestRender(100);
            //TestScenes.ColRowTestRender();
            //TestScenes.RowMetal(1000);
            //TestScenes.RowPlastic(1000);
            //TestScenes.RowTransparentRoughness(100);
            //TestScenes.RowMetalPlastic(100);
            //TestScenes.RowTransparent(100);
            TestScenes.RowTransparentRefraction(100);
            //TestScenes.SkyBoxMappingTestRender();
            //TestScenes.SphereMappingTestRender();
            //TestScenes.InsideSphere();
            //TestScenes.MappingTestRender();
            //TestScenes.TestRender(10);
            //TestScenes.TeapotTest();
            //TestScenes.SolidTestRender(500);
            //TestScenes.LowPolyTeapotTest();
            System.Console.ReadKey();
        }
    }
}