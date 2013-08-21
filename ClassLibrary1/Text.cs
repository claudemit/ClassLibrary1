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
    class Text : GH_ScriptInstance
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
        private void RunScript(object x, object y, ref object A)
        {
            try
            {
                if (sign)
                {
                    sign = false;
                    Rhino.Input.Custom.GetObject go = new Rhino.Input.Custom.GetObject();
                    go.Get();
                    Rhino.DocObjects.ObjRef or = go.Object(0);
                    ID = or.ObjectId;

                    Rhino.DocObjects.RhinoObject obj = RhinoDoc.ActiveDoc.Objects.Find(ID);
                    TextObject T = (TextObject)obj;
                    TextEntity t = T.TextGeometry;
                    m_tags = new Rhino.Display.Text3d(t.Text, t.Plane, t.TextHeight + 1);
                }
            }
            catch (Exception ex) { Print(ex.ToString()); }
        }

        // <Custom additional code> 
        Guid ID;
        Rhino.Display.Text3d m_tags;
        bool sign = true;
        int time = 0;
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            base.DrawViewportWires(args);
            time++;

            Rhino.DocObjects.RhinoObject obj = RhinoDoc.ActiveDoc.Objects.Find(ID);
            TextObject T = (TextObject)obj;
            TextEntity t = T.TextGeometry;
            m_tags = new Rhino.Display.Text3d(t.Text + " " + time.ToString()
              , t.Plane, t.TextHeight + 1);

            if (this.m_tags != null)
            {
                Color defaultColour = Color.Blue;
                args.Display.Draw3dText(this.m_tags, defaultColour);
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////
    }
}
