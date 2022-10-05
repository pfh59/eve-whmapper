using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WHMapper.Services.Anoik
{
    public interface IAnoikServices
    {
        public Task<string> GetSystemClass(string systemName);
        public Task<string> GetSystemEffects(string systemName);
        public Task<IEnumerable<KeyValuePair<string, string>>> GetSystemStatics(string systemName);
        public Task<IEnumerable<KeyValuePair<string, string>>> GetSystemEffectsInfos(string effectName, string systemClass);
      
    } 
}
