using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour
{

    public GameObject bubblePrefab;
    public GameObject bubbleAreaPrefab;

    private GameState gameState;

    void Start()
    {
        gameState = GameState.Title;
    }

    void Update()
    {
        switch (gameState)
        {
            case GameState.Title:
                UpdateTitle();
                break;
            case GameState.Create:
                UpdateCreate();
                break;
            case GameState.Active:
                UpdateActive();
                break;
            case GameState.Win:
                UpdateWin();
                break;
            default:
                break;
        }
    }

    private void UpdateTitle()
    {
        // If click, goto game

        // TODO: Implement Title
        gameState = GameState.Create;
    }

    private void UpdateWin()
    {
        throw new System.NotImplementedException();
    }

    private void UpdateCreate()
    {
        // Remove all game objects
        var root = gameObject;

        var children = new List<GameObject>();
        foreach (Transform child in transform) { children.Add(child.gameObject); }
        children.ForEach(child => Destroy(child));

        // Create game two areas
        var posA = new Vector2(Random.RandomRange(-20, -10), Random.RandomRange(-10, 10));
        var posB = new Vector2(Random.RandomRange(10, 20), Random.RandomRange(-10, 10));

        var areaA = Instantiate(bubbleAreaPrefab, posA, Quaternion.identity);
        var areaB = Instantiate(bubbleAreaPrefab, posB, Quaternion.identity);

        // Create random number of bubbles in each area
        var valA = Random.RandomRange(1, 9);
        var valB = Random.RandomRange(1, 9);

        while (valA + valB < 11)
        {
            valA = Random.RandomRange(1, 9);
            valB = Random.RandomRange(1, 9);
        }

        CreateBubblesInArea(posA, valA);
        CreateBubblesInArea(posB, valB);

        // Change to active
        gameState = GameState.Active;
    }

    private void CreateBubblesInArea(Vector2 pos, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var bPos = new Vector2(Random.RandomRange(-3, 3) + pos.x, Random.RandomRange(-3, 3) + pos.y);
            Instantiate(bubblePrefab, bPos, Quaternion.identity);
        }
    }

    private void UpdateActive()
    {
        // TODO: Check for win

        //if( )
    }
}

public enum GameState
{

    Title,
    Create,
    Active,
    Win

}
