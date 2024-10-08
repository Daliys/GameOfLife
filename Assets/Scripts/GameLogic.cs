using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class GameLogic : System.IDisposable
{
    private Vector2Int _gridSize;
    private Rule _rule;
    private NativeArray<byte> _grid;
    private NativeArray<byte> _nextGenerationGrid;
    private NativeArray<byte> _newGrid;
    private NativeArray<Color32> _rawColorData;

    private Texture2D _texture;
    private int dataSize;

    public void Initialize(Vector2Int gridSize, Rule rule)
    {
        _gridSize = gridSize;
        _rule = rule;

         dataSize = _gridSize.x * _gridSize.y;
        _grid = new NativeArray<byte>(dataSize, Allocator.Persistent);
        _nextGenerationGrid = new NativeArray<byte>(dataSize, Allocator.Persistent);
        _newGrid = new NativeArray<byte>(dataSize, Allocator.Persistent);


        _texture = new Texture2D(_gridSize.x, _gridSize.y, TextureFormat.RGBA32, false);
        _texture.filterMode = FilterMode.Point;
        _rawColorData = new NativeArray<Color32>(_gridSize.x * _gridSize.y, Allocator.Persistent);
    }


    public void Dispose()
    {
        if (_grid.IsCreated) _grid.Dispose();
        if (_nextGenerationGrid.IsCreated) _nextGenerationGrid.Dispose();
        if (_newGrid.IsCreated) _newGrid.Dispose();
        if (_rawColorData.IsCreated) _rawColorData.Dispose();
        if (_texture != null) Object.Destroy(_texture);
        _rule.Dispose();
    }


    public void GenerateRandomGrid(float fillPercentage)
    {
        for (int i = 0; i < _grid.Length; i++)
        {
            _grid[i] = Random.value < fillPercentage ? (byte)1 : (byte)0;
        }
    }


    public void NextGeneration()
    {
        var calculateNeighboursJob = new CountAliveNeighboursJob
        {
            grid = _grid,
            gridSize = _gridSize,
            result = _nextGenerationGrid
        };

        JobHandle neighboursHandle = calculateNeighboursJob.Schedule(dataSize, 2048);

        var applyRulesJob = new ApplyRulesJob
        {
            grid = _grid,
            neighbourCounts = _nextGenerationGrid,
            surviveRules = _rule.surviveRules,
            bornRules = _rule.bornRules,
            newGrid = _newGrid,
            gridSize = _gridSize
        };

        JobHandle applyRulesHandle = applyRulesJob.Schedule(dataSize, 2048, neighboursHandle);

        applyRulesHandle.Complete();

        SwapGrids();
    }


    private void SwapGrids()
    {
        (_newGrid, _grid) = (_grid, _newGrid);
    }



    [BurstCompile]
    public struct CountAliveNeighboursJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<byte> grid;

        public Vector2Int gridSize;

        [WriteOnly]
        public NativeArray<byte> result;

        public void Execute(int index)
        {
            int x = index / gridSize.y;
            int y = index % gridSize.y;

            int aliveNeighbours = 0;


            for (int i = -1; i <= 1; i++)
            {
                int neighbourX = (x + i + gridSize.x) % gridSize.x;
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;

                    int neighbourY = (y + j + gridSize.y) % gridSize.y;
                    aliveNeighbours += grid[neighbourX * gridSize.y + neighbourY];
                }
            }

            result[index] = (byte)aliveNeighbours;
        }
    }


    [BurstCompile]
    public struct ApplyRulesJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<byte> grid;

        [ReadOnly]
        public NativeArray<byte> neighbourCounts;

        [ReadOnly]
        public NativeArray<bool> surviveRules;

        [ReadOnly]
        public NativeArray<bool> bornRules;

        [WriteOnly]
        public NativeArray<byte> newGrid;

        public Vector2Int gridSize;

        public void Execute(int index)
        {
            byte aliveNeighbours = neighbourCounts[index];
            byte currentState = grid[index];
            byte newState = 0;

            if (currentState == 1)
            {
                if (surviveRules[aliveNeighbours])
                    newState = 1;
            }
            else
            {
                if (bornRules[aliveNeighbours])
                    newState = 1;
            }

            newGrid[index] = newState;
        }
    }



    [BurstCompile]
    public struct PrepareRawColorDataJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<byte> grid;

        [WriteOnly]
        public NativeArray<Color32> rawColorData;
        public Color32 aliveColor;
        public Color32 deadColor;
        public int width;  // Ширина сетки
        public int height; // Высота сетки

        public void Execute(int index)
        {
            int y = index / width;
            int x = index % width;
            int gridIndex = x * height + y; // Столбцовый порядок

            byte state = grid[gridIndex];
            rawColorData[index] = state == 1 ? aliveColor : deadColor;
        }
    }

    public void ApplyGridValues()
    {

        var prepareRawColorJob = new PrepareRawColorDataJob
        {
            grid = _grid,
            rawColorData = _rawColorData,
            deadColor = Colors.InactiveColor,
            aliveColor = Colors.ActiveColor,
            width = _gridSize.x,  // Передача ширины сетки
            height = _gridSize.y  // Передача высоты сетки
        };

        // Планирование Job
        JobHandle prepareColorHandle = prepareRawColorJob.Schedule(_grid.Length, 2048);

        // Ожидание завершения Job
        prepareColorHandle.Complete();

        //     for(int x = 0; x < _gridSize.x; x++)
        //     {
        //         for(int y = 0; y < _gridSize.y; y++)
        //         {
        //             _texture.SetPixel(x, y, _rawColorData[x * _gridSize.y + y]);
        //         }
        //     }
        //    _texture.Apply();



       _texture.SetPixelData(_rawColorData, 0);
        _texture.Apply();
    }


    public bool GetCellState(int x, int y)
    {
        return _grid[x * _gridSize.y + y] == 1;
    }


    public Texture2D GetTexture()
    {
        return _texture;
    }
}
