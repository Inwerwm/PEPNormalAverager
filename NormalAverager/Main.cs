using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PEPlugin;
using PEPlugin.Pmx;
using PEPExtensions;
using KdTree;
using PEPlugin.SDX;

namespace NormalAverager
{
    public class NormalAverager : PEPluginClass
    {
        public NormalAverager() : base()
        {
        }

        public override string Name
        {
            get
            {
                return "選択頂点内の同位置頂点の法線を平均化";
            }
        }

        public override string Version
        {
            get
            {
                return "0.0";
            }
        }

        public override string Description
        {
            get
            {
                return "選択頂点内の同位置頂点の法線を平均化";
            }
        }

        public override IPEPluginOption Option
        {
            get
            {
                // boot時実行, プラグインメニューへの登録, メニュー登録名
                return new PEPluginOption(false, true, "選択頂点内の同位置頂点の法線を平均化");
            }
        }

        public override void Run(IPERunArgs args)
        {
            try
            {
                var pmx = args.Host.Connector.Pmx.GetCurrentState();
                var selectedVertices = args.Host.Connector.View.PmxView.GetSelectedVertexIndices().Select(i => pmx.Vertex[i]);

                var tree = new KdTree<float, IPXVertex>(3, new KdTree.Math.FloatMath(), AddDuplicateBehavior.List);
                foreach (var v in selectedVertices)
                {
                    tree.Add(v.Position.ToArray(), v);
                }
                foreach (var n in tree.DuplicatedNodes)
                {
                    var sum = n.Duplicate.Aggregate(new V3(0, 0, 0), (a, v) => a + v.Normal);

                    foreach (var v in n.Duplicate)
                    {
                        v.Normal = sum / n.Duplicate.Count;
                    }
                }

                Utility.Update(args.Host.Connector, pmx, PmxUpdateObject.Vertex);
            }
            catch (Exception ex)
            {
                Utility.ShowException(ex);
            }
        }
    }
}
