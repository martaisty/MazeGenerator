﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MazeGenerator.Generate;
using MazeGenerator.Types;

namespace MazeGenerator.Searchers
{
	class ModifiedBFS : Searcher
	{
		private bool[,] deadBlocks;
		public ModifiedBFS(Generator generator) : base(generator) { }
		public override void Search(ref bool? canDoNextStep)
		{
			paths.Clear();
			deadBlocks = new bool[generator.height, generator.width];
			SetDeadBlocks();
			Path startPath = new Path();
			//DFS(ref canDoNextStep, generator.start, startPath);
		}

		protected override void SearchAsync(IProgress<string> progress, ManualResetEvent signal)
		{
			paths.Clear();
			deadBlocks = new bool[generator.height, generator.width];
			SetDeadBlocks();
			BFS(progress, signal);
		}
		private void BFS(IProgress<string> progress, ManualResetEvent signal)
		{
			//Progress
			progress?.Report($"Started search");
			signal?.Reset();
			signal?.WaitOne();
			//Initialize path
			Queue<Path> allPossiblePaths = new Queue<Path>();
			Path tmp = new Path();
			tmp.AddPoint(generator.start);
			allPossiblePaths.Enqueue(tmp);
			//Search
			while(allPossiblePaths.Count != 0)
			{
				tmp = allPossiblePaths.Dequeue();
				Point lastPoint = tmp.path.Last();
				if (lastPoint.x == generator.finish.x && lastPoint.y == generator.finish.y)
				{
					paths.Add(tmp);
				}
				else
				{
					//left
					if (!generator.mapMatrix[lastPoint.y, lastPoint.x].left && lastPoint.x > 0 &&
						IsNotVisited(tmp, new Point { x = (ushort)(lastPoint.x - 1), y = lastPoint.y} ))
					{
						Path p = new Path(tmp);
						p.AddPoint(new Point { x = (ushort)(lastPoint.x - 1), y = lastPoint.y });
						allPossiblePaths.Enqueue(p);
					}
					//up
					if (!generator.mapMatrix[lastPoint.y, lastPoint.x].up && lastPoint.y > 0 &&
						IsNotVisited(tmp, new Point { x = lastPoint.x, y = (ushort)(lastPoint.y - 1) }))
					{
						Path p = new Path(tmp);
						p.AddPoint(new Point { x = lastPoint.x, y = (ushort)(lastPoint.y - 1) });
						allPossiblePaths.Enqueue(p);
					}
					//right
					if (!generator.mapMatrix[lastPoint.y, lastPoint.x].right && lastPoint.x < generator.width - 1 &&
						IsNotVisited(tmp, new Point { x = (ushort)(lastPoint.x + 1), y = lastPoint.y }))
					{
						Path p = new Path(tmp);
						p.AddPoint(new Point { x = (ushort)(lastPoint.x + 1), y = lastPoint.y });
						allPossiblePaths.Enqueue(p);
					}
					//down
					if (!generator.mapMatrix[lastPoint.y, lastPoint.x].down && lastPoint.y < generator.height - 1 &&
						IsNotVisited(tmp, new Point { x = lastPoint.x, y = (ushort)(lastPoint.y + 1) }))
					{
						Path p = new Path(tmp);
						p.AddPoint(new Point { x = lastPoint.x, y = (ushort)(lastPoint.y + 1) });
						allPossiblePaths.Enqueue(p);
					}
					//Progress
					List<Path> tmpPaths = new List<Path>(paths);
					paths.Clear();
					foreach (Path p in allPossiblePaths)
						paths.Add(p);
					progress?.Report($"Searching...");
					paths.Clear();
					foreach (Path p in tmpPaths)
						paths.Add(p);
					signal?.Reset();
					signal?.WaitOne();
				}
			}
			//Progress
			progress?.Report($"Search has ended");
			signal?.Dispose();
		}
		private bool IsNotVisited(Path p, Point point) => !p.ContainsPoint(point) && !deadBlocks[point.y, point.x];
		private void SetBlankAsDeadBlock(ushort y, ushort x)
		{
			if (y >= deadBlocks.GetLength(0) || x >= deadBlocks.GetLength(1))
				return;
			if (deadBlocks[y, x])
				return;
			int k = 0;
			if (generator.mapMatrix[y, x].left || (x > 0 && deadBlocks[y, x - 1])) ++k;
			if (generator.mapMatrix[y, x].up || (y > 0 && deadBlocks[y - 1, x])) ++k;
			if (generator.mapMatrix[y, x].right || (x < generator.width - 1 && deadBlocks[y, x + 1])) ++k;
			if (generator.mapMatrix[y, x].down || (y < generator.height - 1 && deadBlocks[y + 1, x])) ++k;
			if (k >= 3)
			{
				deadBlocks[y, x] = true;
				if (!generator.mapMatrix[y, x].left) SetBlankAsDeadBlock(y, (ushort)(x - 1));
				if (!generator.mapMatrix[y, x].up) SetBlankAsDeadBlock((ushort)(y - 1), x);
				if (!generator.mapMatrix[y, x].right) SetBlankAsDeadBlock(y, (ushort)(x + 1));
				if (!generator.mapMatrix[y, x].down) SetBlankAsDeadBlock((ushort)(y + 1), x);
			}
		}
		private void SetDeadBlocks()
		{
			for (int i = 0; i < deadBlocks.GetLength(0); ++i)
				for (int j = 0; j < deadBlocks.GetLength(1); ++j)
					SetBlankAsDeadBlock((ushort)i, (ushort)j);
		}
	}
}
