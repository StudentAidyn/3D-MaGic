using UnityEngine;
using Newtonsoft.Json.Linq;

public interface ISaveable
{
    public JToken Save();
    public void Load(JToken token);
}
