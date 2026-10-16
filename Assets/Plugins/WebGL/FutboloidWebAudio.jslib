mergeInto(LibraryManager.library, {
  Futboloid_HasWebAudioGesture: function () {
    return (typeof window !== 'undefined' && window.__futboloidWebAudioGesture === true) ? 1 : 0;
  }
});
