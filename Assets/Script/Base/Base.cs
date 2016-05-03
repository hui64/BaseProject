using UnityEngine;
using System.Collections;

public class Base  {
    private static Base Instance;
    public Base() {
                
    }
    public static Base getInstance() {
        if (Instance != null)
            return new Base();
        return Instance;
    }


}
