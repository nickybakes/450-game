using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridAutoSpacing : MonoBehaviour
{
    private GridLayoutGroup grid;

    private float defaultYSpacing;

    // Start is called before the first frame update
    void Start()
    {
        grid = GetComponent<GridLayoutGroup>();
        defaultYSpacing = grid.spacing.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.childCount <= 4)
        {
            grid.spacing = new Vector2(grid.spacing.x, defaultYSpacing);
        }
        if (transform.childCount > 4 && transform.childCount <= 6)
        {
            grid.spacing = new Vector2(grid.spacing.x, defaultYSpacing * .38f);
        }
        else if (transform.childCount > 6 && transform.childCount <= 8)
        {
            grid.spacing = new Vector2(grid.spacing.x, defaultYSpacing * .09f);
        }
    }
}
