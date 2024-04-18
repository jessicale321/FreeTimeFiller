using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UserTask
{
    public class SpecialTask : Task
    {
        private SpecialTaskData specialTaskData;

        protected override void OnCheckboxClick()
        {
            base.OnCheckboxClick();

            // Load minigame scene!
            if (specialTaskData != null)
            {
                Debug.Log($"Load scene for {specialTaskData.sceneName}");

                SceneManager.LoadSceneAsync(specialTaskData.sceneName, LoadSceneMode.Additive);
            }
            
        }
        public override void CompleteOnCommand()
        {
            base.CompleteOnCommand();

            // Don't allow user to interact with checkbox anymore
            checkBoxButton.interactable = false;
        }

        public override void UpdateTask(TaskData data, TaskPlacer taskPlacer)
        {
            base.UpdateTask(data, taskPlacer);

            specialTaskData = data as SpecialTaskData;
        }

    }
}
