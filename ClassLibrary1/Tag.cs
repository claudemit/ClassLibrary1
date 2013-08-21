using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Collections;
using Rhino.Display;
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
    class Tag : GH_ScriptInstance
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
        private void RunScript(List<Object> x, ref object A, ref object B, ref object C)
        {
            try
            {
                m_tags = new List<Rhino.Display.Text3d>();
                // List<int> index = new List<int>(); List<Point3d> pos = new List<Point3d>();
                for (int i = 0; i < x.Count; i++)
                {
                    Point3d p;
                    // index.Add(i);
                    if (x[i].GetType() == typeof(Line))
                    {
                        Line l = (Line)(x[i]);
                        p = (l.From + l.To) / 2;
                    }
                    else
                    {
                        Rhino.Geometry.GeometryBase base1 = (Rhino.Geometry.GeometryBase)x[i];
                        // pos.Add(base1.GetBoundingBox(true).Center);

                        p = base1.GetBoundingBox(true).Center;
                    }

                    Rhino.Display.Text3d text = new Rhino.Display.Text3d(i.ToString(), new Plane(p, Vector3d.ZAxis), 1);
                    m_tags.Add(text);
                }
                //A = m_tags;
            }
            catch (Exception ex) { Print(ex.ToString()); }
            //////////////////////////////////////////////////////
        }
        ////////////////////////////////////////////////////////////////////////////////////
        //代码
        List<Text3d> m_tags ;
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            base.DrawViewportWires(args);
            if (this.m_tags != null)
            {
                Color defaultColour=Color.Blue;
                //= args.WireColour;
              //  defaultColour = args.WireColour_Selected;

                int Count = this.m_tags.Count - 1;
                for (int i = 0; i <= Count; i++)
                {
                    Color col = defaultColour;
                    args.Display.Draw3dText(this.m_tags[i], col);
                }
            }
        }

       public  void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<Guid> obj_ids)
{
        List<Text3d>.Enumerator tag;
        if (att == null)
        {
            att = doc.CreateDefaultAttributes();
        }
        try
        {
            tag = this.m_tags.GetEnumerator();
            while (tag.MoveNext())
            {
                Text3d tag3d = tag.Current;
                Guid id = doc.Objects.AddText(tag3d, att);
                if (!(id == Guid.Empty))
                {
                    obj_ids.Add(id);
                }
            }
        }
        finally
        {
           tag.Dispose();
        } 
}
        ////////////////////////////////////////////////////////////////////////////////////
    }
}
