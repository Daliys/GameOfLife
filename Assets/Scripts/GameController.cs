using System.Collections;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject _cellPrefab;
    [SerializeField] private Transform _gridParent;
    [SerializeField] private Vector2Int _gridSize;
    [SerializeField] private float _fillPercentage;

    [SerializeField] private RawImage _rawImage;
    [SerializeField] private TMP_Text fpsText;

    private GameLogic _gameLogic;
    private Rule _rule;
    private int frames = 0;

    private void Awake()
    {
        Screen.SetResolution(3024, 1964, true);
        _rule = new Rule(new int[] { 2, 3 }, new int[] { 3 }, Allocator.Persistent);

        _gameLogic = new GameLogic();
        _gameLogic.Initialize(_gridSize, _rule);
        _gameLogic.GenerateRandomGrid(_fillPercentage);
        _rawImage.texture = _gameLogic.GetTexture();
        StartCoroutine(FrameCounter());
    }


    private float updateInterval = 1.0f / 20.0f; 
    private float lastUpdateTime = 0.0f;



    private void Update()
    {

        lastUpdateTime += Time.deltaTime;

        while (lastUpdateTime >= updateInterval)
        {
            lastUpdateTime -= updateInterval;
            //float startTime = Time.realtimeSinceStartup;
            _gameLogic.NextGeneration();
            //float middleTime = Time.realtimeSinceStartup;
            // middleValueTotal += middleTime - startTime;
            // count++;
            _gameLogic.ApplyGridValues();
            //float endTime = Time.realtimeSinceStartup;
            // Debug.Log($"NextGeneration: {(middleTime - startTime) * 1000}, ApplyGridValues: {(endTime - middleTime) * 1000}");
            frames++;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            frames = 0;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private IEnumerator FrameCounter()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.9999f);
            //Debug.Log($"FPS: {frames}");
            fpsText.text = $"FPS: {frames}";
            frames = 0;
            //Debug.Log($"Middle: {middleValueTotal / count * 1000}");
            //middleValueTotal = 0;
            //count = 0;
        }
    }

    private void OnDestroy()
    {
        _gameLogic.Dispose();
    }
}

