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
public class graph2 : GH_ScriptInstance
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
  private void RunScript(List<Line> x, Point3d y, ref object A, ref object B, ref object C)
  {


    try
    {
      /////-------------------------------------------------------------------
      // <Custom additional code>
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

      id.Sort(CompareDinosByLength2);
      List<Line> output1 = new   List<Line>();
      for(int i = 0;i < id.Count;i++){
        output1.Add(new Line(vs[id[i].I].pos, vs[id[i].J].pos));
      }


      for (int j = 0; j < vs.Count; j++)
      {
        if (vs[j].refer.Count == 1) { vs[j].energe = 1; }
        if(y.DistanceTo(vs[j].pos) < 0.01){vs[j].energe = 0;signStart = j;}
      }
      for (int i = 0; i < 12; i++)
      {
        bool sign = true;
        for (int j = 0; j < vs.Count; j++)
        {
          if (Vertice.Spread(ref vs, j)) sign = false;
        }
        if (sign) break;
      }
      //////
      loop(ref vs, signStart);
      //////
      List<Point3d> out1 = new List<Point3d>();
      List<string> out2 = new List<string>();
   /*   for (int j = 0; j < id.Count; j++)
      {
        double e1 = vs[id[j].I].energe;
        double e2 = vs[id[j].J].energe;
        double t = e1;
        if(t > e2)t = e2;
        if(t > 50)t = 50;
        out2.Add(t);
        out1.Add((vs[id[j].I].pos + vs[id[j].J].pos) / 2);
      }
  */
     // vs.Sort(CompareDinosByLength);检测用，使用后id失效
      for (int j = 0; j < vs.Count; j++)
      {
        out2.Add(vs[j].Sequece.ToString());
        out1.Add(vs[j].pos);
      }


      A = out1; B = out2;C = output1;
      ////---------------------------------------------------------------------
    }
    catch (Exception ex)
    {
      Print(ex.ToString());
    }

  }

  // <Custom additional code> 
  int signStart = 0;
  public int Count = 0;

  private static int CompareDinosByLength2(IndexPair x, IndexPair y)
  {
    if(Pmin(x) > Pmin(y))return 1;
    if (Pmin(x) < Pmin(y)) return -1;
    if(Pmin(x) == Pmin(y)){
      if(Pmax(x) > Pmax(y))return 1;
      if (Pmax(x) < Pmax(y)) return -1;
      return 0;
    }
    else return 0;

  }
  static int Pmin(IndexPair pair){
    if(pair.I > pair.J)return pair.J;
    else return pair.I;
  }

  static int Pmax(IndexPair pair){
    if(pair.I > pair.J)return pair.I;
    else return pair.J;
  }


  private void loop(ref List<Vertice> vs, int Index)
  {
    vs[Index].Sequece = Count; Count++;
    for (int i = 0; i < vs[Index].refer.Count; i++)
    {
      int t = vs[Index].refer[i];
      if (vs[t].energe <= vs[Index].energe)
      {
        Print(Count.ToString() + "----");
        if(Count < 1000 && t != signStart){
          loop(ref vs, t);}
      }
    }
  }
  class Vertice

  {
    public double energe = 0;
    /// <summary>
    /// spred 识别为只有1true的情况
    /// </summary>
    /////////////////////
    public Point3d pos;
    public bool dead = false;
    public List<int> refer = new List<int>();
    public int Sequece = -1;

    public static bool Spread(ref List<Vertice> vs, int thisIndex)
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
        vs[id1].energe += vs[thisIndex].energe;
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
  private static int CompareDinosByLength(Vertice x, Vertice y)
  {
    if (x == null) { if (y == null) { return 0; } else { return -1; } }
    else
    {
      if (y == null) { return 1; }
      else
      {
        if (x.Sequece > y.Sequece) return 1;
        if (x.Sequece == y.Sequece) return 0;
        if (x.Sequece < y.Sequece) return -1;
        else return 0;
      }
    }
  }
  // </Custom additional code> 
}