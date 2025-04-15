using System;
using System.Collections.Generic;
using System.Linq;
using FuzzySharp;
using GalaxyWiki.Core.Entities;
using GalaxyWiki.Core.DTOs;

namespace GalaxyWiki.Core.Services
{
    public class SearchService
    {
        public IEnumerable<(T Item, string MatchType, int MatchRatio)> SearchCelestialBodies<T>(IEnumerable<T> bodies, string searchTerm) where T : CelestialBodies
        {
            var results = new List<(T Item, string MatchType, int MatchRatio)>();
            
            foreach (var body in bodies)
            {
                // Check for exact match
                if (body.BodyName.Equals(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add((body, "Exact", 100));
                    continue;
                }
                
                // Check for contains
                if (body.BodyName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add((body, "Contains", 90));
                    continue;
                }
                
                // Check for fuzzy match
                int ratio = Fuzz.Ratio(body.BodyName.ToLower(), searchTerm.ToLower());
                if (ratio >= 60) // Only include if match ratio is at least 60%
                {
                    results.Add((body, "Fuzzy", ratio));
                }
            }
            
            return results.OrderByDescending(r => r.MatchRatio);
        }
        
        public IEnumerable<(StarSystems Item, string MatchType, int MatchRatio)> SearchStarSystems(IEnumerable<StarSystems> systems, string searchTerm)
        {
            var results = new List<(StarSystems Item, string MatchType, int MatchRatio)>();
            
            foreach (var system in systems)
            {
                // Check for exact match
                if (system.Name.Equals(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add((system, "Exact", 100));
                    continue;
                }
                
                // Check for contains
                if (system.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add((system, "Contains", 90));
                    continue;
                }
                
                // Check for fuzzy match
                int ratio = Fuzz.Ratio(system.Name.ToLower(), searchTerm.ToLower());
                if (ratio >= 60) // Only include if match ratio is at least 60%
                {
                    results.Add((system, "Fuzzy", ratio));
                }
            }
            
            return results.OrderByDescending(r => r.MatchRatio);
        }
        
        public IEnumerable<(CommentDto Item, string MatchType, int MatchRatio)> SearchCommentDtos(IEnumerable<CommentDto> comments, string searchTerm)
        {
            var results = new List<(CommentDto Item, string MatchType, int MatchRatio)>();
            
            foreach (var comment in comments)
            {
                // Check for exact match in comment text
                if (comment.CommentText.Equals(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add((comment, "Exact", 100));
                    continue;
                }
                
                // Check for contains in comment text
                if (comment.CommentText.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add((comment, "Contains", 90));
                    continue;
                }
                
                // Check for fuzzy match in comment text
                int ratio = Fuzz.Ratio(comment.CommentText.ToLower(), searchTerm.ToLower());
                if (ratio >= 60) // Only include if match ratio is at least 60%
                {
                    results.Add((comment, "Fuzzy", ratio));
                }
            }
            
            return results.OrderByDescending(r => r.MatchRatio);
        }
    }
} 