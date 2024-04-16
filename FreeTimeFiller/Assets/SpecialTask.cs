using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UserTask
{
    public class SpecialTask : Task
    {
        private SpecialTaskData specialTaskData;

        protected override void OnCheckboxClick()
        {
            base.OnCheckboxClick();

            if(specialTaskData != null)
            {
                Debug.Log($"Load scene for {specialTaskData.sceneIndex}");
            }
            
        }

        public override void UpdateTask(TaskData data, TaskPlacer taskPlacer)
        {
            base.UpdateTask(data, taskPlacer);

            specialTaskData = data as SpecialTaskData;
        }

    }
}
