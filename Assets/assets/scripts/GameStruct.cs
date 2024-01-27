/*feelings = ['Interessiert', 'Traurig', 'Wütend', 'Gelangweilt', 'Lachend'];

wordsToAccept = ['Finger', 'Tier', 'Löffel'..];
wordsToReject = ['Autobahn', 'Straße', 'Hund'];

-> ANALYZE
:: Max 5 von GPT condition, dass das feeling getroffen wurde
:: + 5
:: - 2
*/

using System.Threading.Tasks;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

[System.Serializable]
public class CriteriaStruct {
  public string feeling;
  public string[] noGos;
  public string[] toUse;
}

public class GameStruct : MonoBehaviour
{

  public TMP_Text feeling;
  public TMP_Text noGos;
  public TMP_Text toUse;

  public CriteriaStruct[] crits;

  public CriteriaStruct currentCrits;

  void Start()
  {
    
  }

  public CriteriaStruct selectNewGameCriteria() {
    int l = crits.Length - 1;
    Debug.Log("crits.Length" + crits.Length);
    if (this.crits.Length > 0) {
      int n = Random.Range(0, this.crits.Length);
      this.currentCrits = this.crits[n];
      fillFE();
      return this.crits[n];
    } else {
      CriteriaStruct c = new CriteriaStruct();
      c.feeling = "sad";
      c.noGos = new string[] { "Finger", "Tier", "Löffel", "Tee", "Buchstabe" };
      c.toUse = new string[] {"Autobahn", "Straße", "Hund", "Kaffee", "Tütü"};
      this.currentCrits = c;
      fillFE();
      return c;
    }
  }

  public void fillFE() {
    feeling.text = this.currentCrits.feeling;
    if (this.currentCrits.feeling == "Interessiert") {
      feeling.text = "Super Interessant";
    }
    if (this.currentCrits.feeling == "Traurig") {
      feeling.text = "Zeit für tränen...";
    }
    if (this.currentCrits.feeling == "Wütend") {
      feeling.text = "Richtig Bösartig...";
    }
    if (this.currentCrits.feeling == "Gelangweilt") {
      feeling.text = "Etwas langweiliges...";
    }
    if (this.currentCrits.feeling == "Lachend") {
      feeling.text = "Lustig Lustig Lustig...";
    }
    string combinedString = string.Join(",", this.currentCrits.noGos);
    string combinedString1 = string.Join(",", this.currentCrits.toUse);
    noGos.text = combinedString;
    toUse.text = combinedString1;
  }
}
