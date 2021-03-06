﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;
using System.Linq;
using Newtonsoft.Json;

public class Region
{
    public static Behavior behavior;
    public static Resource.Definitions resourceDefinitions;

    public readonly int id;
    public List<Site> sites = new List<Site>();
    public int capital;
    public Map map { get; }
    public Resources resources;

    public Topography topography = new Topography();

    Kingdom owner = null;

    public Region(int id, Map _map, List<Site> _sites, int _capital = 0)
    {
        this.id = id;
        sites = _sites;
        capital = _capital;
        map = _map;

        LoadResourcesDefinitions();
    }

    void LoadResourcesDefinitions()
    {
        resources = new Resources();

        foreach (var def in resourceDefinitions.Keys) {
            resources.Add(def, new Resource(resourceDefinitions[def]));
        }
    }

    public class Frontiers
    {
        public List<Edge> innerEdges;
        public List<Edge> outerEdges;
    }

    public Frontiers GetFrontiers()
    {
        var edges = new List<Edge>();

        foreach (Site site in sites) {
            foreach (Edge edge in site.Edges) {
                edges.Add(edge);
            }
        }

        List<Edge> internalEdges = new List<Edge>();
        List<Edge> finalEdges = new List<Edge>();
        foreach (Edge edge in edges) {
            if (internalEdges.Contains(edge)) {
                finalEdges.RemoveAll(o => o == edge);
                continue;
            }
            internalEdges.Add(edge);
            finalEdges.Add(edge);
        }

        internalEdges.RemoveAll(o => finalEdges.Contains(o));

        return new Frontiers() { innerEdges = internalEdges, outerEdges = finalEdges };
    }

    public List<Region> GetNeighbors()
    {
        List<Region> neighbors = new List<Region>();

        List<Site> neighborSites = GetNeighborSites();
        foreach (Site site in sites) {
            foreach (Site neighborSite in site.NeighborSites()) {
                if (!sites.Contains(neighborSite) && !neighborSites.Contains(neighborSite)) {
                    neighborSites.Add(neighborSite);
                }
            }
        }

        foreach (Site neighborSite in neighborSites) {
            foreach (Region region in map.regions) {
                if (region.sites.Contains(neighborSite) && !neighbors.Contains(region)) {
                    neighbors.Add(region);
                }
            }
        }

        return neighbors;
    }

    public List<Site> GetNeighborSites()
    {
        List<Site> neighborSites = new List<Site>();
        foreach (Site site in sites) {
            foreach (Site neighborSite in site.NeighborSites()) {
                if (!sites.Contains(neighborSite) && !neighborSites.Contains(neighborSite)) {
                    neighborSites.Add(neighborSite);
                }
            }
        }
        return neighborSites;
    }

    public bool IsOwned()
    {
        return owner != null;
    }

    public void MakeOwnedBy(Kingdom kingdom)
    {
        kingdom.TakeOwnership(this);
    }

    public void RemoveOwnership()
    {
        if (IsOwned())
        {
            owner.RemoveOwnership(this);
        }
        owner = null;
    }

    public void SetRawOwner(Kingdom newOwner=null)
    {
        owner = newOwner;
    }

    public Kingdom GetOwner()
    {
        return owner;
    }

    public class Behavior
    {
        public System.Action<Region> onGameStart;
        public System.Action<Region> onNewDay;
        public System.Action<Region> onNewMonth;
        public System.Action<Region> onNewYear;
    }

    public class Topography
    {
        public float elevation = 0f;
        public float moisture = 0f;
        public float temperature = 0f;
    }
}
