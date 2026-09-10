using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurveUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform drawingArea;

    [Header("Curve Position")]
    [SerializeField] private float startX = 5f;
    [SerializeField] private float startY = 280f;
    [SerializeField] private float xStep = 75f;

    [Header("Movement Heights")]
    [SerializeField] private float strongUpHeight = 80f;
    [SerializeField] private float upHeight = 40f;
    [SerializeField] private float downHeight = 40f;
    [SerializeField] private float strongDownHeight = 80f;

    [Header("Viewport")]
    [SerializeField] private float rightMargin = 50f;
    [SerializeField] private float topMargin = 50f;
    [SerializeField] private float bottomMargin = 50f;

    [Header("Visual")]
    [SerializeField] private float lineThickness = 5f;

    [Header("Animation")]
    [SerializeField] private float drawDuration = 0.4f;

    private readonly List<GameObject> createdSegments = new List<GameObject>();

    public void DrawCurve(IReadOnlyList<CurveMovement> movements)
    {
        ClearCurve();

        if (drawingArea == null) return;

        List<Vector2> logicalPoints = BuildLogicalPoints(movements);

        float horizontalOffset = CalculateHorizontalOffset(logicalPoints);
        float verticalOffset = CalculateVerticalOffset(logicalPoints);

        for (int i = 0; i < movements.Count; i++)
        {
            Vector2 start = logicalPoints[i] - new Vector2(horizontalOffset, verticalOffset);
            Vector2 end = logicalPoints[i + 1] - new Vector2(horizontalOffset, verticalOffset);

            bool animate = i == movements.Count - 1;

            CreateSegment(start, end, movements[i], animate);
        }
    }

    private List<Vector2> BuildLogicalPoints(IReadOnlyList<CurveMovement> movements)
    {
        List<Vector2> points = new List<Vector2>();

        Vector2 currentPoint = new Vector2(startX, startY);
        points.Add(currentPoint);

        for (int i = 0; i < movements.Count; i++)
        {
            float nextX = currentPoint.x + xStep;
            float nextY = currentPoint.y + GetMovementHeight(movements[i]);

            currentPoint = new Vector2(nextX, nextY);
            points.Add(currentPoint);
        }

        return points;
    }

    private float CalculateHorizontalOffset(List<Vector2> points)
    {
        if (points.Count == 0) return 0f;

        float width = drawingArea.rect.width;
        float lastX = points[points.Count - 1].x;
        float maximumVisibleX = width - rightMargin;

        return Mathf.Max(0f, lastX - maximumVisibleX);
    }

    private float CalculateVerticalOffset(List<Vector2> points)
    {
        if (points.Count == 0) return 0f;

        float height = drawingArea.rect.height;
        float lastY = points[points.Count - 1].y;

        float minimumVisibleY = bottomMargin;
        float maximumVisibleY = height - topMargin;

        if (lastY > maximumVisibleY)
            return lastY - maximumVisibleY;

        if (lastY < minimumVisibleY)
            return lastY - minimumVisibleY;

        return 0f;
    }

    private float GetMovementHeight(CurveMovement movement)
    {
        switch (movement)
        {
            case CurveMovement.StrongUp:
                return strongUpHeight;

            case CurveMovement.Up:
                return upHeight;

            case CurveMovement.Stable:
                return 0f;

            case CurveMovement.Down:
                return -downHeight;

            case CurveMovement.StrongDown:
                return -strongDownHeight;

            default:
                return 0f;
        }
    }

    private void CreateSegment(Vector2 start, Vector2 end, CurveMovement movement, bool animate)
    {
        GameObject segment = new GameObject("CurveSegment", typeof(RectTransform), typeof(Image));
        segment.transform.SetParent(drawingArea, false);

        RectTransform rect = segment.GetComponent<RectTransform>();
        Image image = segment.GetComponent<Image>();

        Vector2 direction = end - start;

        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = start;
        rect.sizeDelta = new Vector2(animate ? 0f : distance, lineThickness);
        rect.localRotation = Quaternion.Euler(0f, 0f, angle);

        image.color = GetMovementColor(movement);
        image.raycastTarget = false;

        createdSegments.Add(segment);

        if (animate)
            StartCoroutine(AnimateSegment(rect, distance));
    }

    private IEnumerator AnimateSegment(RectTransform rect, float targetWidth)
    {
        float elapsedTime = 0f;

        while (elapsedTime < drawDuration)
        {
            if (rect == null) yield break;

            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / drawDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            rect.sizeDelta = new Vector2(targetWidth * smoothT, lineThickness);

            yield return null;
        }

        if (rect != null)
            rect.sizeDelta = new Vector2(targetWidth, lineThickness);
    }

    private Color GetMovementColor(CurveMovement movement)
    {
        switch (movement)
        {
            case CurveMovement.StrongUp:
            case CurveMovement.Up:
                return new Color(0.1f, 0.65f, 0.25f);

            case CurveMovement.Down:
            case CurveMovement.StrongDown:
                return new Color(0.85f, 0.15f, 0.1f);

            case CurveMovement.Stable:
                return Color.gray;

            default:
                return Color.black;
        }
    }

    private void ClearCurve()
    {
        StopAllCoroutines();

        foreach (GameObject segment in createdSegments)
        {
            if (segment != null)
                Destroy(segment);
        }

        createdSegments.Clear();
    }
}