using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Collections;
using GH_IO;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Collections;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RhinoDebug
{
    class circles : GH_ScriptInstance
    {
        #region Members
        /// <summary>List of error messages. Do not modify this list directly.</summary>
        private List<string> __err = new List<string>();

        /// <summary>List of print messages. Do not modify this list directly, use the Print() and Reflect() functions instead.</summary>
        private List<string> __out = new List<string>();

        /// <summary>Represents the current Rhino document.</summary>
        private RhinoDoc doc = RhinoDoc.ActiveDoc;

        /// <summary>Represents the Script component which maintains this script.</summary>

        /// <summary>Represents the number of times that RunScript has been called within this solution.</summary
        private int runCount;
        #endregion

        #region Utility functions
        /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
        /// <param name="text">String to print.</param>
        private void Print(string text)
        {
            __out.Add(text);
        }

        /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
        /// <param name="format">String format.</param>
        /// <param name="args">Formatting parameters.</param>
        private void Print(string format, params object[] args)
        {
            __out.Add(string.Format(format, args));
        }

        /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
        /// <param name="obj">Object instance to parse.</param>
        private void Reflect(object obj)
        {

        }

        /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
        /// <param name="obj">Object instance to parse.</param>
        private void Reflect(object obj, string method_name)
        {

        }
        private void RedefineSolution()
        {
            // owner.ExpireSolution(true);
        }

        #endregion

        private void RunScript(List<Line> x, double y, List<Mesh> z, object u, ref object A, ref object B, ref object C)
        {
            try
            {
                List<IndexPair> id = new List<IndexPair>();
                List<Vertice> vs = new List<Vertice>();
                id.Add(new IndexPair(0, 1));
                ////////////////////////////////////////////////////
                vs.Add(new Vertice(x[0].From, 1));
                vs.Add(new Vertice(x[0].To, 0));
                for (int i = 1; i < x.Count; i++)
                {
                    bool sign1 = true;
                    bool sign2 = true;
                    int a = 0, b = 0;
                    for (int j = 0; j < vs.Count; j++)
                    {
                        if (vs[j].equalTo(x[i].From)) { sign1 = false; a = j; }
                        if (vs[j].equalTo(x[i].To)) { sign2 = false; b = j; }
                        if (!sign1 && !sign2) { break; }
                    }
                    if (sign1) { vs.Add(new Vertice(x[i].From)); a = vs.Count - 1; }
                    if (sign2) { vs.Add(new Vertice(x[i].To)); b = vs.Count - 1; }
                    vs[a].Add(b); vs[b].Add(a);
                    id.Add(new IndexPair(a, b));
                }
                for (int j = 0; j < vs.Count; j++)
                {
                    vs[j].cleanRefer();
                }

                List<Point3d> ps1 = new List<Point3d>();
                List<double> rs1 = new List<double>();
                for (int j = 0; j < vs.Count; j++)
                {
                    if (vs[j].refer.Count == 1)
                    {
                        rs1.Add(vs[j].pos.DistanceTo(vs[vs[j].refer[0]].pos));
                        ps1.Add(vs[j].pos);
                    }
                }
                List<Circle> out1 = new List<Circle>();
                for (int i = 0; i < ps1.Count; i++)
                {
                    double min = double.MaxValue;
                    Vector3d N = new Vector3d();
                    for (int j = 0; j < z.Count; j++)
                    {
                        Mesh mesh = z[j];
                        double t = mesh.ClosestPoint(ps1[i]).DistanceTo(ps1[i]);
                        if (t < min)
                        {
                            min = t; mesh.FaceNormals.ComputeFaceNormals();
                            N = mesh.FaceNormals[0];
                        }
                    }
                    out1.Add(new Circle(new Plane(ps1[i], N), rs1[i] * Math.Sqrt(y) / 5));
                    Print(N.Length.ToString());
                }

                A = out1;
            }
            catch (Exception ex) { Print(ex.ToString()); }
            //////////////////////////////////////////////////////
        }
        ////////////////////////////////////////////////////////////////////////////////////
        //代码
        class Vertice
        {
            public double energe = 0;
            /// <summary>
            /// spred 识别为只有1true的情况
            /// </summary>

            public static bool Spread(ref List<Vertice> vs, int thisIndex, double weight)
            {
                if (vs[thisIndex].energe == 0 || vs[thisIndex].dead) return false;
                int sign = 0; int id1 = 0;
                for (int i = 0; i < vs[thisIndex].refer.Count; i++)
                {
                    if (!vs[vs[thisIndex].refer[i]].dead) { sign++; id1 = vs[thisIndex].refer[i]; }
                }
                if (sign == 1)
                {
                    vs[thisIndex].dead = true;
                    vs[id1].energe += vs[thisIndex].energe * weight;
                    return true;
                }
                return false;
            }

            public static bool Test(ref List<Vertice> vs, int thisIndex)
            {
                if (vs[thisIndex].dead) return false;
                if (vs[thisIndex].refer.Count != 2) return false;
                //   Vector3d v1 = vs[vs[thisIndex].refer[0]].pos - vs[thisIndex].pos;
                // Vector3d v2 = vs[vs[thisIndex].refer[1]].pos - vs[thisIndex].pos;
                // v1.Unitize();v2.Unitize();
                if (vs[thisIndex].refer.Count == 2)
                {
                    //(Vector3d.VectorAngle(v1, v2) < Math.PI / 180 * 30){
                    vs[thisIndex].dead = true;
                    vs[vs[thisIndex].refer[0]].changeConnection(thisIndex, vs[thisIndex].refer[1]);
                    vs[vs[thisIndex].refer[1]].changeConnection(thisIndex, vs[thisIndex].refer[0]);
                    //   vs.RemoveAt(thisIndex);
                    return true;
                }
                return false;
            }
            /////////////////////
            public Point3d pos;
            public bool dead = false;
            public List<int> refer = new List<int>();
            public Vertice(Point3d p)
            {
                pos = new Point3d(p);
            }
            public Vertice(Point3d p, int index)
            {
                pos = new Point3d(p);
                this.refer.Add(index);
            }
            public void changeConnection(int a, int b)
            {
                List<int> newRefer = new List<int>();
                newRefer.Add(b);
                bool sign = false;
                for (int i = 0; i < this.refer.Count; i++)
                {
                    if (this.refer[i] != a) { newRefer.Add(refer[i]); }
                    else { sign = true; }
                }
                if (sign) this.refer = newRefer;
            }
            public void Add(int i)
            {
                this.refer.Add(i);
            }
            public bool equalTo(Point3d pt)
            {
                if (pos.DistanceTo(pt) < 0.01) { return true; }
                return false;
            }
            public void cleanRefer()
            {
                if (this.refer.Count < 2) return;
                else if (this.refer.Count == 2)
                {
                    if (this.refer[0] == this.refer[1]) this.refer.RemoveAt(0);
                    return;
                }
                else
                {
                    this.refer.Sort();
                    List<int> newRefer = new List<int>();
                    newRefer.Add(refer[0]);
                    for (int i = 1; i < refer.Count - 1; i++)
                    {
                        if (refer[i] != refer[i - 1] && refer[i] != refer[i + 1]) newRefer.Add(refer[i]);
                    }
                    if (refer[refer.Count - 2] != refer[refer.Count - 1]) newRefer.Add(refer[refer.Count - 1]);
                    this.refer = newRefer;
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////
    }
}
