using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webbAPI.Models;

namespace webbAPI.DataService
{
    public class SharedDB
    {
        private readonly ConcurrentDictionary<string, UserConnection> _connection = new();
        public ConcurrentDictionary<string, UserConnection> Connection => _connection;
    }
}