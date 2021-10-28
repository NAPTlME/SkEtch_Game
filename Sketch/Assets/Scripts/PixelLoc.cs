using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using QuikGraph;

public class PixelLoc
{
    private int _xPos;
    private int _yPos;
    public int xPos { get => _xPos; }
    public int yPos { get => _yPos; }
    public Color color;

    public PixelLoc(int xPos, int yPos, Color color)
    {
        _xPos = xPos;
        _yPos = yPos;
        this.color = color;
    }

    public bool IsNeighbor(PixelLoc otherPixel)
    {
        return Math.Abs(this.xPos - otherPixel.xPos) <= 1 && Math.Abs(this.yPos - otherPixel.yPos) <= 1;
    }
}

[System.Serializable]
public class SerializablePixelLoc
{
    private int _xPos;
    private int _yPos;
    public int xPos { get => _xPos; }
    public int yPos { get => _yPos; }
    public float _r;
    public float _g;
    public float _b;
    public float _a;

    public SerializablePixelLoc(PixelLoc pixel)
    {
        _xPos = pixel.xPos;
        _yPos = pixel.yPos;
        _r = pixel.color.r;
        _g = pixel.color.g;
        _b = pixel.color.b;
        _a = pixel.color.a;
    }

    public bool IsNeighbor(PixelLoc otherPixel)
    {
        return Math.Abs(this.xPos - otherPixel.xPos) <= 1 && Math.Abs(this.yPos - otherPixel.yPos) <= 1;
    }

    public PixelLoc ConvertToPixelLoc()
    {
        return new PixelLoc(xPos, yPos, new Color(_r, _g, _b, _a));
    }
}

[System.Serializable]
public class TextureSaveFormat
{
    public int resolution;
    public List<List<SerializablePixelLoc>> pixelSeq;

    public TextureSaveFormat(int resolution, List<List<SerializablePixelLoc>> pixelSeq)
    {
        this.resolution = resolution;
        this.pixelSeq = pixelSeq.ToList();
    }

    public List<List<PixelLoc>> ConvertToPixelLoc()
    {
        return pixelSeq.Select(sel => sel.Select(sel2 => sel2.ConvertToPixelLoc()).ToList()).ToList();
    }
}

public static class PixelLocUtility
{
    public static List<List<PixelLoc>> groupPixelsByNeighborsAndCalcDrawOrder(List<PixelLoc> pixels)
    {
        Dictionary<int, List<int>> indexGrouping = new Dictionary<int, List<int>>();
        Dictionary<int, int[]> pixelOutEdges = new Dictionary<int, int[]>();
        // convert pixels to a dictionary where the key is an integer
        var pixelIndex = pixels.Select((pixel, index) => (pixel, index)).ToList();

        int nextKey = 0;
        // separate by color and then by neighbors
        foreach(var colorGrp in pixelIndex.GroupBy(grp => grp.pixel.color))
        {
            Debug.Log("Group count: " + colorGrp.Count());
            for (int i = 0; i < colorGrp.Count(); i++)
            {
                //var graph = new BidirectionalGraph<PixelLoc, Edge<PixelLoc>>();
                
                var remainingPixels = colorGrp.Skip(i).ToList();
                // get distances of this pixel to all other pixels
                var indicesThatAreNeighbors = remainingPixels.Where(sel => sel.pixel.IsNeighbor(colorGrp.ElementAt(i).pixel)).Select(sel => sel.index).ToList(); // this will include self

                // add to out edges dictionary
                pixelOutEdges.Add(colorGrp.ElementAt(i).index, indicesThatAreNeighbors.Where(wh => !wh.Equals(colorGrp.ElementAt(i).index)).ToArray());

                // check if these pixels are in the dictionary
                // if not make a new group, if true add to group
                // also combine groups if there are more than one
                var foundGroups = indexGrouping.Where(wh => wh.Value.Intersect(indicesThatAreNeighbors).Any());
                if (foundGroups.Count() > 0)
                {
                    // take the first and remove the rest after adding the rest to the first
                    int key = foundGroups.First().Key;

                    List<int> value = indicesThatAreNeighbors;
                    foreach (var grp in foundGroups)
                    {
                        value.AddRange(grp.Value);
                    }
                    indexGrouping[key] = value.Distinct().ToList();
                    foreach (var grp in foundGroups.Skip(1).Select(sel => sel.Key).ToList())
                    {
                        indexGrouping.Remove(grp);
                    }
                }
                else
                {
                    indexGrouping.Add(nextKey, indicesThatAreNeighbors);
                    nextKey++;
                }
            }
        }



        // now convert indices to pixelLocs for return
        List<List<(PixelLoc pixel, int index)>> pixelGroups = new List<List<(PixelLoc pixel, int index)>>();
        foreach (int i in indexGrouping.Keys)
        {
            pixelGroups.Add(indexGrouping[i].Select(j => pixelIndex[j]).ToList());
        }

        List<List<PixelLoc>> returnVal = CalculateDrawOrder(pixelGroups, pixelOutEdges);


        return returnVal;
    }

    private static List<List<PixelLoc>> CalculateDrawOrder(List<List<(PixelLoc pixel, int index)>> pixelGroups, Dictionary<int, int[]> pixelOutEdges)
    {
        List<List<PixelLoc>> returnVal = new List<List<PixelLoc>>();

        foreach(var pixelList in pixelGroups)
        {
            var pixelDict = pixelList.ToDictionary(t => t.index, t => t.pixel);
            List<PixelLoc> orderedList = new List<PixelLoc>();
            if (pixelList.Count() == 1)
            {
                orderedList.Add(pixelList.First().pixel);
            }
            else
            {
                List<int> untouchedVertices = new List<int>();
                untouchedVertices.AddRange(pixelDict.Keys.ToArray());
                // create graph of connected pixels from group
                var edges = pixelOutEdges.Where(wh => pixelDict.ContainsKey(wh.Key))
                            .SelectMany(sel => sel.Value.Select(sel2 => new Edge<int>(sel.Key, sel2))).ToArray();

                var graph = edges.ToBidirectionalGraph<int, Edge<int>>();

                // decide where to start
                // get the lowest degree and take one as the start (nice for getting end pionts (degree == 1)
                int startingVertex = pixelDict.Keys.Select(sel => (sel, graph.Degree(sel))).OrderBy(ord => ord.Item2).First().sel;
                orderedList.Add(pixelDict[startingVertex]);
                untouchedVertices.Remove(startingVertex);
                int previousVertex = startingVertex;
                int i = 0;
                int tot = untouchedVertices.Count();
                while (untouchedVertices.Count() > 0 && i <= tot)
                {
                    var nextVertex = FindNearestTarget(new int[] { previousVertex }, pixelDict, untouchedVertices.ToArray(), graph, new List<int>());
                    orderedList.Add(pixelDict[nextVertex]);
                    untouchedVertices = untouchedVertices.Where(wh => !wh.Equals(nextVertex)).ToList();
                    previousVertex = nextVertex;
                    //untouchedVertices.Remove(nextVertex);
                     i++;
                }
                if (untouchedVertices.Count() > 0)
                {
                    Debug.LogError("untouched vertices: " + untouchedVertices.Count());
                    Debug.LogError("Original count: " + tot);
                }
            }
            returnVal.Add(orderedList);
        }

        return returnVal;
    }
    private static int FindNearestTarget(int[] startingNodes, Dictionary<int, PixelLoc> allNodes, int[] targetNodes, BidirectionalGraph<int, Edge<int>> graph, List<int> visitedNodes)
    {
        Dictionary<int, List<int>> adjacentNodes = new Dictionary<int, List<int>>();
        // simple implementation of a BFS where we short circuit on the first hit of a target
        foreach(var startingNode in startingNodes)
        {
            visitedNodes.Add(startingNode);
            var allAdjacentNodes = new List<int>();
            allAdjacentNodes.AddRange(graph.OutEdges(startingNode).Select(sel => sel.Target));
            allAdjacentNodes.AddRange(graph.InEdges(startingNode).Select(sel => sel.Source));
            foreach(var node in allAdjacentNodes)
            {
                if (adjacentNodes.ContainsKey(node))
                {
                    adjacentNodes[node].Add(startingNode);
                }
                else
                {
                    if(!visitedNodes.Contains(node))
                    {
                        adjacentNodes.Add(node, new List<int>() { startingNode });
                    }
                }
            }
        }
        var foundTargets = adjacentNodes.Keys.Intersect(targetNodes);
        if(foundTargets.Count() > 1)
        {
            // prioritize nodes that are left, left-upper, up, right-upper, etc in a clockwise fashion
            // to do this, get angle from the values -x, y
            // also note that in this use case all values x and y are -1 to 1
            return foundTargets.Select(sel =>
            {
                var thisPixel = allNodes[sel];
                var angles = adjacentNodes[sel].Select(sel2 =>
                {
                    var parentNode = allNodes[sel2];
                    var dy = thisPixel.yPos - parentNode.yPos;
                    var dx = parentNode.xPos - thisPixel.xPos;
                    var theta = Math.Atan2(dy, dx);
                    return theta;
                });
                return (sel, angles.Min());
            }).OrderBy(ord => ord.Item2).First().sel;
        }
        else if (foundTargets.Count() > 0)
        {
            return foundTargets.First();
        }
        else
        {
            return FindNearestTarget(adjacentNodes.Keys.ToArray(), allNodes, targetNodes, graph, visitedNodes);
        }
    }
}
