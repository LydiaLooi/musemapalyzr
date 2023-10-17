using System;
using System.Windows.Forms;
using OxyPlot.WindowsForms;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Legends;
using static MuseMapalyzr.ConfigReader;

namespace MuseMapalyzr
{
    public class MultiplierGraphing
    {

        private static MuseMapalyzrConfig Conf = ConfigReader.GetConfig();
        private static MuseMapalyzrConfig UnrankedConf = ConfigReader.GetUnrankedConfig();

        private static MuseMapalyzrConfig GetRightConfig(bool ranked)
        {
            if (ranked) return Conf;
            return UnrankedConf;
        }


        public static PlotModel GraphMethods(bool ranked)
        {
            PlotModel plotModel = new PlotModel
            {
                Title = "Multiplier vs Note Speed (NPS)",
            };
            Legend legend = new Legend { LegendTitle = "Patterns", LegendPosition = 0 };
            plotModel.Legends.Add(legend);

            int count = 1000;
            double[] npsValues = new double[count];
            double step = 50 / (double)count;

            for (int i = 0; i < count; i++)
            {
                npsValues[i] = 1 + i * step;
            }

            AddLineSeries(plotModel, npsValues, (nps) => PatternMultiplier.EvenCircleMultiplier(nps, ranked), "Even Circle");
            AddLineSeries(plotModel, npsValues, (nps) => PatternMultiplier.SkewedCircleMultiplier(nps, ranked), "Skewed Circle");
            AddLineSeries(plotModel, npsValues, (nps) => PatternMultiplier.ZigZagMultiplier(nps, ranked), "Zig Zag");
            AddLineSeries(plotModel, npsValues, (nps) => PatternMultiplier.NothingButTheoryMultiplier(nps, ranked), "Nothing But Theory");
            AddLineSeries(plotModel, npsValues, (nps) => PatternMultiplier.StreamMultiplier(nps, ranked), "Stream");
            AddLineSeries(plotModel, npsValues, (nps) => PatternMultiplier.FourStackMultiplier(nps, ranked), "4-Stacks");
            AddLineSeries(plotModel, npsValues, (nps) => PatternMultiplier.ThreeStackMultiplier(nps, ranked), "3-Stacks");
            AddLineSeries(plotModel, npsValues, (nps) => PatternMultiplier.TwoStackMultiplier(nps, ranked), "2-Stacks");
            AddLineSeries(plotModel, npsValues, (nps) => PatternMultiplier.VaryingStacksMultiplier(nps, ranked), "Varying Stacks");

            // Uncomment to see the zig zag length multiplier
            //AddLineSeries(plotModel, npsValues, (nps) => PatternMultiplier.ZigZagLengthMultiplier(nps, GetRightConfig(ranked).ZigZagLengthNpsThreshold + 1, ranked), "Zig Zag Length Multiplier");


            return plotModel;
        }

        private static void AddLineSeries(PlotModel model, double[] npsValues, Func<double, double> method, string label)
        {
            var lineSeries = new LineSeries { Title = label };

            foreach (double nps in npsValues)
            {
                lineSeries.Points.Add(new DataPoint(nps, method(nps)));
            }

            model.Series.Add(lineSeries);
        }

        public static void Graph(bool ranked)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = new MyForm(ranked);
            Application.Run(form); // This line actually shows the form
        }

        public partial class MyForm : Form
        {
            public MyForm(bool ranked)
            {

                var plot = new PlotView
                {
                    Model = GraphMethods(ranked),
                    Dock = DockStyle.Fill
                };

                Controls.Add(plot);
            }
        }
    }

}