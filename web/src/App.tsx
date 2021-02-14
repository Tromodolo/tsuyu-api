import React from 'react';
import Router from './Router';
import { InitPersist } from './storage';

InitPersist();

function App() {
  return (
    <Router></Router>
  );
}

export default App;
