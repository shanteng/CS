using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SMVC.Patterns;

public class BaseRemoteProxy : Proxy 
{
    private static readonly object sycObj = new object();
    public BaseRemoteProxy(string name) : base(name)
    {
        NAME = name;
    }
}
