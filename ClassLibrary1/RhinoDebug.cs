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
    class RhinoDebugDemo:GH_ScriptInstance
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

        private void RunScript(List<Mesh> x, List<int> y, object z, ref object A, ref object B, ref object C)
        {
            try
            {

                List<Line> output1 = new List<Line>();
                List<double> output2 = new List<double>();
                List<Point3d> output3 = new List<Point3d>();
                for (int i = 0; i < x.Count; i++)
                {
                    List<Line> output11;
                    List<double> output22;
                    Point3d output33;
                    Print(Step(x[i], y[i], 3, true, out output11, out output22, out output33));
                    output1.AddRange(output11);
                    output2.AddRange(output22);
                    output3.Add(output33);
                }
                A = output1; B = output2; C = output3;
            }
            catch (Exception ex)
            {
                Print(ex.ToString());
            }

        }
        ////////////////////////////////////////////////////////////////////////////////////
        //代码

        public String Step(Mesh mesh, int age, double dis, bool type, out List<Line> out1, out List<double> out2, out Point3d IO)
        {
            string str = "";
            List<Line> output1 = new List<Line>();
            List<double> output2 = new List<double>();
            IO = new Point3d();
            if (mesh.Vertices.Count < 4)
            {
                str = "Mesh vertices Count<4"; out1 = output1; out2 = output2; return str;
            }
            if (age < 2)
            {
                str = "age<2"; out1 = output1; out2 = output2; return str;
            }
            mesh.FaceNormals.ComputeFaceNormals();
            Polyline pl = new Polyline();
            pl.Add(mesh.Vertices[0]);
            pl.Add(mesh.Vertices[1]);
            pl.Add(mesh.Vertices[2]);
            pl.Add(mesh.Vertices[3]);


            List<List<box>> tree = new List<List<box>>();

            for (int i = 0; i < age; i++)
            {
                List<box> branch = new List<box>();
                Polyline poly1 = new Polyline(pl);
                poly1.Transform(Transform.Translation((Vector3d)mesh.FaceNormals[0] * dis * i));
                branch.Add(new box(poly1, Math.Pow(2, age - 2)));
                Loop(ref branch, age - i - 1, type);
                tree.Add(branch);
            }

            for (int i = 1; i < age; i++)
            {
                List<box> branch2 = tree[i - 1];
                List<box> branch = tree[i];
                for (int j = 0; j < branch.Count; j++)
                {
                    output1.Add(new Line(branch[j].p, branch2[j * 2].p));
                    output2.Add(branch[j].age);
                    output1.Add(new Line(branch[j].p, branch2[j * 2 + 1].p));
                    output2.Add(branch[j].age);
                }
            }

            out1 = output1; out2 = output2; IO = tree[tree.Count - 1][0].p;
            return str;
        }
        public void Loop(ref List<box> b, int step, bool type)
        {
            for (int j = 0; j < step; j++)
            {
                List<box> output = new List<box>();
                for (int i = 0; i < b.Count; i++)
                {
                    if (type)
                    {
                        output.AddRange(b[i].splitQuad());
                    }
                    else
                    {
                        output.AddRange(b[i].splitTriangle());
                    }
                }
                b = output;
            }
        }
        public class box : Polyline
        {
            public double age = 100;
            public Point3d p = new Point3d();
            public box(Polyline l, double Age)
            {
                this.age = Age;
                this.AddRange(l);
                for (int i = 0; i < l.Count; i++)
                {
                    p += this[i];
                }
                p /= this.Count;
            }
            public List<box> splitTriangle()
            {
                List<box> output = new List<box>();
                Polyline l1 = new Polyline();
                Polyline l2 = new Polyline();
                if (this.Count == 4)
                {
                    Point3d p0 = this[0];
                    Point3d p1 = this[1];
                    Point3d p2 = this[2];
                    Point3d p3 = this[3];
                    l1.Add(p0);
                    l1.Add(p1);
                    l1.Add(p3);
                    l2.Add(p1);
                    l2.Add(p2);
                    l2.Add(p3);
                }
                else
                {
                    Point3d p0 = this[0];
                    Point3d p1 = this[1];
                    Point3d p2 = this[2];
                    Point3d p3 = (p0 + p1) / 2;
                    double maxD = p0.DistanceTo(p1);
                    int sign = 1;
                    if (p1.DistanceTo(p2) > maxD) { sign = 2; maxD = p1.DistanceTo(p2); p3 = (p1 + p2) / 2; }
                    if (p2.DistanceTo(p0) > maxD) { sign = 3; p3 = (p2 + p0) / 2; }
                    if (sign == 1)
                    {
                        l1.Add(p0);
                        l1.Add(p2);
                        l1.Add(p3);
                        l2.Add(p3);
                        l2.Add(p2);
                        l2.Add(p1);
                    }
                    else if (sign == 2)
                    {
                        l1.Add(p0);
                        l1.Add(p1);
                        l1.Add(p3);
                        l2.Add(p0);
                        l2.Add(p3);
                        l2.Add(p2);

                    }
                    else
                    {
                        l1.Add(p0);
                        l1.Add(p3);
                        l1.Add(p1);
                        l2.Add(p3);
                        l2.Add(p2);
                        l2.Add(p1);

                    }
                }
                output.Add(new box(l1, this.age / 2)); output.Add(new box(l2, this.age / 2));
                return output;
            }
            public List<box> splitQuad()
            {
                List<box> output = new List<box>();
                Point3d p0 = this[0];
                Point3d p1 = this[1];
                Point3d p2 = this[2];
                Point3d p3 = this[3];
                Polyline l1 = new Polyline();
                Polyline l2 = new Polyline();
                Point3d p4, p5;
                if (p0.DistanceTo(p1) > p1.DistanceTo(p2))
                {
                    p4 = p0 * 0.5 + p1 * 0.5;
                    p5 = p3 * 0.5 + p2 * 0.5;
                    l1.Add(p0); l1.Add(p4); l1.Add(p5); l1.Add(p3);
                    l2.Add(p4); l2.Add(p1); l2.Add(p2); l2.Add(p5);
                }
                else
                {
                    p4 = p0 * 0.5 + p3 * 0.5;
                    p5 = p1 * 0.5 + p2 * 0.5;
                    l1.Add(p0); l1.Add(p1); l1.Add(p5); l1.Add(p4);
                    l2.Add(p4); l2.Add(p5); l2.Add(p2); l2.Add(p3);
                }
                output.Add(new box(l1, this.age / 2)); output.Add(new box(l2, this.age / 2));
                return output;
            }
        }


        ////////////////////////////////////////////////////////////////////////////////////
    }
}
