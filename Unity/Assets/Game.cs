using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Game : MonoBehaviour
{

    public GameObject bubblePrefab;
    public GameObject bubbleAreaPrefab;

    private List<BubbleAreaPhysics> bubbleAreas = new List<BubbleAreaPhysics>();
    private int total;

    private GameState gameState;
    private GameState lastGameState;

    private BackgroundChanger backgroundChanger;

    void Start()
    {
        gameState = GameState.Title;
        backgroundChanger = transform.root.GetComponentInChildren<BackgroundChanger>();
    }


    void Update()
    {
        var hasChanged = lastGameState != gameState;
        lastGameState = gameState;

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
                UpdateWin(hasChanged);
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

    private void UpdateCreate()
    {
        // Change background
        backgroundChanger.ChangeBackground();

        // Remove all game objects
        var root = gameObject;

        var children = new List<GameObject>();
        foreach (Transform child in transform) { children.Add(child.gameObject); }
        children.ForEach(child =>
        {
            Destroy(child);
        });

        // Create game two areas
        var posA = new Vector2(Random.RandomRange(-20, -10), Random.RandomRange(-10, 10));
        var posB = new Vector2(Random.RandomRange(10, 20), Random.RandomRange(-10, 10));

        var areaA = Instantiate(bubbleAreaPrefab, posA, Quaternion.identity) as GameObject;
        var areaB = Instantiate(bubbleAreaPrefab, posB, Quaternion.identity) as GameObject;

        areaA.transform.parent = root.transform;
        areaB.transform.parent = root.transform;

        bubbleAreas.Clear();
        bubbleAreas.Add(areaA.GetComponent<BubbleAreaPhysics>());
        bubbleAreas.Add(areaB.GetComponent<BubbleAreaPhysics>());


        // Create random number of bubbles in each area
        var valA = Random.RandomRange(1, 9);
        var valB = Random.RandomRange(1, 9);

        while (valA + valB < 11)
        {
            valA = Random.RandomRange(1, 9);
            valB = Random.RandomRange(1, 9);
        }

        total = valA + valB;

        CreateBubblesInArea(root, posA, valA);
        CreateBubblesInArea(root, posB, valB);

        // Change to active
        gameState = GameState.Active;
    }

    private void CreateBubblesInArea(GameObject root, Vector2 pos, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var bPos = new Vector2(Random.RandomRange(-3, 3) + pos.x, Random.RandomRange(-3, 3) + pos.y);
            var bubble = Instantiate(bubblePrefab, bPos, Quaternion.identity) as GameObject;
            bubble.transform.parent = root.transform;
        }
    }

    private float timeAtCorrect;

    private void UpdateActive()
    {
        // Check for win
        var isCorrect = false;

        if (BubblePhysics.LiveBubbles.All(b => b.IsAttachedToBubbleArea))
        {
            if (bubbleAreas.Any(a => a.attachedBubbles.Count == 10))
            {
                isCorrect = true;

                if (timeAtCorrect == 0)
                {
                    timeAtCorrect = Time.time;
                }
                else if (Time.time - timeAtCorrect > 1f)
                {
                    bubbleAreas.ForEach(a => a.LockBubbles());
                    gameState = GameState.Win;
                }
            }
        }

        if (!isCorrect) { timeAtCorrect = 0; }
    }

    private float timeAtWin;

    private void UpdateWin(bool hasChanged)
    {
        if (hasChanged)
        {
            timeAtWin = Time.time;

            // Show win
            var winArea = bubbleAreas.First(a => a.attachedBubbles.Count == 10);
            var loseArea = bubbleAreas.First(a => a != winArea);

            // Remove inner bubbles
            foreach (var b in winArea.attachedBubbles)
            {
                b.gameObject.SetActive(false);
            }

            loseArea.gameObject.GetComponent<BubbleText>().text = "" + (total - 10);

            // Intersect with small bubbles
            //winArea.gameObject.layer = winArea.attachedBubbles.First().gameObject.layer;

            // Disolve other areas
            //bubbleAreas.Where(a => a != winArea).ToList().ForEach(a => a.gameObject.SetActive(false));
        }

        var timePassed = Time.time - timeAtWin;

        if (timePassed > 3)
        {
            gameState = GameState.Create;
        }
    }
}

public enum GameState
{

    Title,
    Create,
    Active,
    Win

}
