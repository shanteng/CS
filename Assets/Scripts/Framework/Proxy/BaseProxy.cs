using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SMVC.Patterns;

public class BaseRemoteProxy : Proxy
{
    public BaseRemoteProxy(string name) : base(name)
    {
        NAME = name;
    }
}
