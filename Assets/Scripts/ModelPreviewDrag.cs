using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ModelPreviewDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] ModelPreviewController previewController;
    [SerializeField] float dragSensitivity = 0.35f;
    [SerializeField] float resumeAutoRotateDelay = 0.5f;

    Coroutine resumeRoutine;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (resumeRoutine != null)
        {
            StopCoroutine(resumeRoutine);
            resumeRoutine = null;
        }

        if (previewController != null)
            previewController.SetInteractionActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (previewController == null)
            return;

        previewController.RotateModel(-eventData.delta.x * dragSensitivity);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (previewController == null)
            return;

        if (resumeAutoRotateDelay <= 0f)
        {
            previewController.SetInteractionActive(false);
            return;
        }

        resumeRoutine = StartCoroutine(ResumeAutoRotateAfterDelay());
    }

    IEnumerator ResumeAutoRotateAfterDelay()
    {
        yield return new WaitForSeconds(resumeAutoRotateDelay);
        if (previewController != null)
            previewController.SetInteractionActive(false);
        resumeRoutine = null;
    }
}
