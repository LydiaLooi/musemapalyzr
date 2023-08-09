using System;
using System.Windows.Forms;
using OxyPlot.WindowsForms;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Legends;
namespace MuseMapalyzr
{
    public class MultiplierGraphing
    {
        public static PlotModel GraphMethods()
        {
            PlotModel plotModel = new PlotModel
            {
                Title = "Multiplier vs Note Speed (NPS)",
            };
            Legend legend = new Legend { LegendTitle = "Patterns", LegendPosition = 0 };
            plotModel.Legends.Add(legend);

            int count = 1000;
            double[] npsValues = new double[count];
            double step = (50 - 1) / (double)count;

            for (int i = 0; i < count; i++)
            {
                npsValues[i] = 1 + i * step;
            }

            AddLineSeries(plotModel, npsValues, PatternMultiplier.EvenCircleMultiplier, "Even Circle");
            AddLineSeries(plotModel, npsValues, PatternMultiplier.SkewedCircleMultiplier, "Skewed Circle");
            AddLineSeries(plotModel, npsValues, PatternMultiplier.ZigZagMultiplier, "Zig Zag");
            AddLineSeries(plotModel, npsValues, PatternMultiplier.NothingButTheoryMultiplier, "Nothing But Theory");
            AddLineSeries(plotModel, npsValues, PatternMultiplier.StreamMultiplier, "Stream");
            AddLineSeries(plotModel, npsValues, PatternMultiplier.FourStackMultiplier, "4-Stacks");
            AddLineSeries(plotModel, npsValues, PatternMultiplier.ThreeStackMultiplier, "3-Stacks");
            AddLineSeries(plotModel, npsValues, PatternMultiplier.TwoStackMultiplier, "2-Stacks");
            AddLineSeries(plotModel, npsValues, PatternMultiplier.VaryingStacksMultiplier, "Varying Stacks");


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

        public static void Graph()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = new MyForm();
            Application.Run(form); // This line actually shows the form
        }

        public partial class MyForm : Form
        {
            public MyForm()
            {

                var plot = new PlotView
                {
                    Model = GraphMethods(),
                    Dock = DockStyle.Fill
                };

                Controls.Add(plot);
            }
        }
    }

}