using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TreeNode
{
    bool _alive;
    TreeNode _tnw, _tne, _tsw, _tse, _dnw, _dne, _dsw, _dse;
    int _depth;
    int _population;


    public TreeNode(bool alive)
    {
        _alive = alive;
        _tnw= _tne= _tsw= _tse= _dnw= _dne= _dsw= _dse = null;
        _depth = 0;
        _population = alive ? 1 : 0;
    }

    public TreeNode(TreeNode tnw, TreeNode tne, TreeNode tsw, TreeNode tse, TreeNode dnw, TreeNode dne, TreeNode dsw, TreeNode dse) 
    {
        _tnw = tnw;
        _tne = tne;
        _tsw = tsw;
        _tse = tse;
        _dnw = dnw;
        _dne = dne;
        _dsw = dsw;
        _dse = dse;
        _depth = tnw._depth + 1;

    }

}

