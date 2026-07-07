using System;
using System.Collections.Generic;
using UnityEngine;

namespace Futboloid.Gameplay.Defenders
{
    /// <summary>Собирает расстановку из фигур со смещениями: якорь на сетке + остаток одиночками.</summary>
    public static class DefenderFormationComposer
    {
        private const int MaxPlacementAttempts = 48;

        public static List<int> Compose(
            DefenderGenerationSettings settings,
            int tier,
            int targetCount,
            in PlacementZoneDefinition zone,
            System.Random rng)
        {
            var result = new List<int>(targetCount);
            if (targetCount <= 0 || settings == null)
                return result;

            var columns = settings.GridColumns;
            var occupied = new HashSet<long>();
            var remaining = targetCount;

            while (remaining > 0)
            {
                var before = result.Count;

                if (!TryPlaceNextShape(settings, tier, remaining, zone, columns, occupied, result, rng))
                    TryPlaceSingleton(zone, columns, occupied, result, rng);

                if (result.Count == before)
                {
                    Debug.LogWarning(
                        "[DefenderFormationComposer] Stuck placing defenders; " +
                        $"placed {result.Count}/{targetCount}.");
                    break;
                }

                remaining = targetCount - result.Count;
            }

            return result;
        }

        private static bool TryPlaceNextShape(
            DefenderGenerationSettings settings,
            int tier,
            int remaining,
            in PlacementZoneDefinition zone,
            int columns,
            HashSet<long> occupied,
            List<int> result,
            System.Random rng)
        {
            var formations = settings.Formations;
            if (formations == null || formations.Length == 0)
                return false;

            var candidates = new List<FormationShapeDefinition>();
            for (var i = 0; i < formations.Length; i++)
            {
                var shape = formations[i];
                if (shape.MinTier > tier || shape.Size <= 0 || shape.Size > remaining)
                    continue;

                if (shape.Weight <= 0f)
                    continue;

                candidates.Add(shape);
            }

            if (candidates.Count == 0)
                return false;

            candidates.Sort((a, b) => b.Size.CompareTo(a.Size));

            for (var attempt = 0; attempt < Mathf.Min(6, candidates.Count); attempt++)
            {
                var shape = PickWeightedShape(candidates, remaining, rng);
                if (shape.Size <= 0)
                    continue;

                if (TryPlaceShapeAtRandomAnchor(shape, zone, columns, occupied, result, rng))
                    return true;

                candidates.Remove(shape);
                if (candidates.Count == 0)
                    break;
            }

            return false;
        }

        private static FormationShapeDefinition PickWeightedShape(
            List<FormationShapeDefinition> candidates,
            int remaining,
            System.Random rng)
        {
            var totalWeight = 0f;
            for (var i = 0; i < candidates.Count; i++)
            {
                var shape = candidates[i];
                totalWeight += shape.Weight * ShapeFitMultiplier(shape.Size, remaining);
            }

            if (totalWeight <= 0f)
                return candidates[0];

            var roll = (float)rng.NextDouble() * totalWeight;
            for (var i = 0; i < candidates.Count; i++)
            {
                var shape = candidates[i];
                roll -= shape.Weight * ShapeFitMultiplier(shape.Size, remaining);
                if (roll <= 0f)
                    return shape;
            }

            return candidates[candidates.Count - 1];
        }

        private static float ShapeFitMultiplier(int shapeSize, int remaining)
        {
            if (shapeSize > remaining)
                return 0f;

            if (shapeSize == remaining)
                return 2.5f;

            if (remaining - shapeSize <= 2)
                return 1.6f;

            return 1f;
        }

        private static bool TryPlaceShapeAtRandomAnchor(
            in FormationShapeDefinition shape,
            in PlacementZoneDefinition zone,
            int columns,
            HashSet<long> occupied,
            List<int> result,
            System.Random rng)
        {
            if (shape.Cells == null || shape.Cells.Length == 0)
                return false;

            ComputeBounds(shape.Cells, out var minCol, out var maxCol, out var minRow, out var maxRow);

            var anchorMinCol = zone.MinCol - minCol;
            var anchorMaxCol = zone.MaxCol - maxCol;
            var anchorMinRow = zone.MinRow - minRow;
            var anchorMaxRow = zone.MaxRow - maxRow;

            if (anchorMinCol > anchorMaxCol || anchorMinRow > anchorMaxRow)
                return false;

            var anchorCandidates = new List<Vector2Int>();
            for (var row = anchorMinRow; row <= anchorMaxRow; row++)
            {
                for (var col = anchorMinCol; col <= anchorMaxCol; col++)
                    anchorCandidates.Add(new Vector2Int(col, row));
            }

            Shuffle(anchorCandidates, rng);
            var attempts = Mathf.Min(MaxPlacementAttempts, anchorCandidates.Count);

            for (var i = 0; i < attempts; i++)
            {
                var anchor = anchorCandidates[i];
                if (!CanPlaceShape(shape, anchor, zone, occupied))
                    continue;

                PlaceShape(shape, anchor, columns, occupied, result);
                return true;
            }

            return false;
        }

        private static void TryPlaceSingleton(
            in PlacementZoneDefinition zone,
            int columns,
            HashSet<long> occupied,
            List<int> result,
            System.Random rng)
        {
            var cell = PickScatterCell(zone, occupied, rng);
            if (!cell.HasValue)
                return;

            var packed = DefenderGrid.PackCell(cell.Value.x, cell.Value.y);
            if (!occupied.Add(packed))
                return;

            result.Add(DefenderGrid.ToSlotId(cell.Value.x, cell.Value.y, columns));
        }

        private static Vector2Int? PickScatterCell(
            in PlacementZoneDefinition zone,
            HashSet<long> occupied,
            System.Random rng)
        {
            var freeCells = new List<Vector2Int>();
            for (var row = zone.MinRow; row <= zone.MaxRow; row++)
            {
                for (var col = zone.MinCol; col <= zone.MaxCol; col++)
                {
                    if (!occupied.Contains(DefenderGrid.PackCell(col, row)))
                        freeCells.Add(new Vector2Int(col, row));
                }
            }

            if (freeCells.Count == 0)
                return null;

            if (occupied.Count == 0)
                return freeCells[rng.Next(freeCells.Count)];

            var bestScore = float.MinValue;
            var bestCandidates = new List<Vector2Int>();

            for (var i = 0; i < freeCells.Count; i++)
            {
                var cell = freeCells[i];
                var score = MinDistanceToOccupied(cell, occupied);
                if (score > bestScore + 0.001f)
                {
                    bestScore = score;
                    bestCandidates.Clear();
                    bestCandidates.Add(cell);
                }
                else if (Mathf.Abs(score - bestScore) <= 0.001f)
                {
                    bestCandidates.Add(cell);
                }
            }

            return bestCandidates[rng.Next(bestCandidates.Count)];
        }

        private static float MinDistanceToOccupied(Vector2Int cell, HashSet<long> occupied)
        {
            var best = float.MaxValue;
            foreach (var packed in occupied)
            {
                DefenderGrid.UnpackCell(packed, out var col, out var row);
                var dx = cell.x - col;
                var dy = cell.y - row;
                var dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist < best)
                    best = dist;
            }

            return best;
        }

        private static bool CanPlaceShape(
            in FormationShapeDefinition shape,
            Vector2Int anchor,
            in PlacementZoneDefinition zone,
            HashSet<long> occupied)
        {
            for (var i = 0; i < shape.Cells.Length; i++)
            {
                var offset = shape.Cells[i];
                var col = anchor.x + offset.Col;
                var row = anchor.y + offset.Row;

                if (!zone.Contains(col, row))
                    return false;

                if (occupied.Contains(DefenderGrid.PackCell(col, row)))
                    return false;
            }

            return true;
        }

        private static void PlaceShape(
            in FormationShapeDefinition shape,
            Vector2Int anchor,
            int columns,
            HashSet<long> occupied,
            List<int> result)
        {
            for (var i = 0; i < shape.Cells.Length; i++)
            {
                var offset = shape.Cells[i];
                var col = anchor.x + offset.Col;
                var row = anchor.y + offset.Row;
                var packed = DefenderGrid.PackCell(col, row);
                occupied.Add(packed);
                result.Add(DefenderGrid.ToSlotId(col, row, columns));
            }
        }

        private static void ComputeBounds(
            GridCellOffset[] cells,
            out int minCol,
            out int maxCol,
            out int minRow,
            out int maxRow)
        {
            minCol = int.MaxValue;
            maxCol = int.MinValue;
            minRow = int.MaxValue;
            maxRow = int.MinValue;

            for (var i = 0; i < cells.Length; i++)
            {
                var cell = cells[i];
                if (cell.Col < minCol) minCol = cell.Col;
                if (cell.Col > maxCol) maxCol = cell.Col;
                if (cell.Row < minRow) minRow = cell.Row;
                if (cell.Row > maxRow) maxRow = cell.Row;
            }
        }

        private static void Shuffle<T>(List<T> list, System.Random rng)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
