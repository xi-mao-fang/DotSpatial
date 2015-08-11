﻿using DotSpatial.Topology.Geometries;

namespace DotSpatial.Topology.Noding
{
    ///<summary>
    /// An interface for classes which support adding nodes to a segment string.
    ///</summary>
    public interface INodableSegmentString : ISegmentString
    {
        #region Methods

        ///<summary>Adds an intersection node for a given point and segment to this segment string.
        ///</summary>
        ///<param name="intPt">the location of the intersection</param>
        ///<param name="segmentIndex">the index of the segment containing the intersection</param>
        void AddIntersection(Coordinate intPt, int segmentIndex);

        #endregion
    }
}