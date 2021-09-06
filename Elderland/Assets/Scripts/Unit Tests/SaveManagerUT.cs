using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Standard console output automated tests combined with visual inspect of the files TransferSaveTestFile 1 and 2.
// Must delete the save file "TransferSaveTestFile1.txt", clear TransferSaveTestFile2.txt, and copy and paste 
// TransferSaveControlFile2 into TransferSaveFile2 before running this test to get
// accurate results. 

/*
Expected file structure after running tests:
TransferSaveTestFile1: exists and contains:
1 {"test":1}
2 {"test":2}
(new line here this parenthesis is not in the file)

TransferSaveTestFile2: contains:
3 {"test":3}
(new line here this parenthesis is not in the file)
*/
public class SaveManagerUT : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(TestCoroutine());
    }

    private IEnumerator TestCoroutine()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        try
        {
            SaveManager.TransferObjectToSaveFileTests(GameInfo.SaveManager);

            Debug.Log("SaveManagerUT: Success");
        } 
        catch (Exception e)
        {
            Debug.Log("SaveManagerUT: Failed. " + e.Message + " " + e.StackTrace);
        }
    }
}
