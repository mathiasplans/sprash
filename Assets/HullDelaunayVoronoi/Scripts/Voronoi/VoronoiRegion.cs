using System;
using System.Collections.Generic;

using HullDelaunayVoronoi.Primitives;
using HullDelaunayVoronoi.Delaunay;

namespace HullDelaunayVoronoi.Voronoi
{

    public class VoronoiRegion<VERTEX>
        where VERTEX : class, IVertex, new()
    {

        public int Id { get; set; }

        public IList<DelaunayCell<VERTEX>> Cells { get; private set; }

        public IList<VoronoiEdge<VERTEX>> Edges { get; private set; }

        public VERTEX ArithmeticCenter {get {
            float[] p = this.Cells[0].CircumCenter.Position;

            for (int i = 1; i < this.Cells.Count; ++i) {
                for (int j = 0; j < p.Length; ++j) {
                    p[j] += this.Cells[i].CircumCenter.Position[j];
                }
            }

            for (int i = 0; i < p.Length; ++i) {
                p[i] /= this.Cells.Count;
            }

            VERTEX newp = new VERTEX();
            newp.Position = p;

            return newp;
        }}

        public VoronoiRegion()
        {

            Cells = new List<DelaunayCell<VERTEX>>();

            Edges = new List<VoronoiEdge<VERTEX>>();

        }

        public override string ToString()
        {
            return string.Format("[VoronoiRegion: Id={0}, Cells={1}, Edges={2}]", Id, Cells.Count, Edges.Count);
        }

    }

}
