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



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class graph : GH_ScriptInstance
{
#region Utility functions
  /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
  /// <param name="text">String to print.</param>
  private void Print(string text) { /* Implementation hidden. */ }
  /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
  /// <param name="format">String format.</param>
  /// <param name="args">Formatting parameters.</param>
  private void Print(string format, params object[] args) { /* Implementation hidden. */ }
  /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj) { /* Implementation hidden. */ }
  /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj, string method_name) { /* Implementation hidden. */ }
#endregion

#region Members
  /// <summary>Gets the current Rhino document.</summary>
  private readonly RhinoDoc RhinoDocument;
  /// <summary>Gets the Grasshopper document that owns this script.</summary>
  private readonly GH_Document GrasshopperDocument;
  /// <summary>Gets the Grasshopper script component that owns this script.</summary>
  private readonly IGH_Component Component;
  /// <summary>
  /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
  /// Any subsequent call within the same solution will increment the Iteration count.
  /// </summary>
  private readonly int Iteration;
#endregion

  /// <summary>
  /// This procedure contains the user code. Input parameters are provided as regular arguments,
  /// Output parameters as ref arguments. You don't have to assign output parameters,
  /// they will have a default value.
  /// </summary>
  private void RunScript(List<Circle> x, List<Polyline> y, ref object A, ref object B, ref object C)
  {
      List<Point3d> pos = new List<Point3d>();
      List<string> tag = new List<string>();
      List<Point3d> out1 = new List<Point3d>();
      for (int i = 0; i < y.Count; i++)
      {

          for (int j = 0; j < x.Count; j++)
          {
              if (Inside(y[i], x[j].Center))
              {
                  Point3d ori = CR(y[i]);
                  out1.Add(ori);
                  Vector3d v = x[j].Center - ori;
                  int sx = (int)(v.X * 1000);
                  int sy = (int)(v.Y * -1000);
                  tag.Add("\r\n" + "X=" + sx.ToString()
                    + "\r\n" + "Y=" + sy.ToString()
                    + "\r\n" + "R=" + ((int)(x[j].Radius * 1000)).ToString());
                  pos.Add(x[j].Center);
              }
          }
      }

      A = tag; B = pos; C = out1;
  }

  // <Custom additional code> 
  public Point3d CR(Polyline pl)
  {
      return pl.BoundingBox.Corner(true, false, true);
  }
  public bool Inside(Polyline pl, Point3d testPt)
  {
      BoundingBox box = pl.BoundingBox;
      return box.Contains(testPt);
  }
  // </Custom additional code> 
}