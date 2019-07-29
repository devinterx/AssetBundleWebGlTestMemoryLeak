var MemoryMonitorPlugin = {

  GetTotalMemorySize: function() {
    return TOTAL_MEMORY;
  },

  GetTotalStackSize: function() {
    return TOTAL_STACK;
  },

  GetStaticMemorySize: function() {
    return STATICTOP - STATIC_BASE;
  },

  GetDynamicMemorySize: function() {
    if (typeof DYNAMICTOP !== "undefined") {
      return DYNAMICTOP - DYNAMIC_BASE;
    } else {
      return HEAP32[DYNAMICTOP_PTR >> 2] - DYNAMIC_BASE;
    }
  }
};

mergeInto(LibraryManager.library, MemoryMonitorPlugin);
