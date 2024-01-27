/*feelings = ['happy', 'sad', 'angry', 'laugh'];

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
    if (this.crits.Length > 0) {
      int n = Random.Range(0, this.crits.Length);
      return this.crits[n];
    } else {
      CriteriaStruct c = new CriteriaStruct();
      c.feeling = "sad";
      c.noGos = new string[] { "Finger", "Tier", "Löffel", "Tee", "Buchstabe" };
      c.toUse = new string[] {"Autobahn", "Straße", "Hund"};
      this.currentCrits = c;
      return c;
    }
  }
}
