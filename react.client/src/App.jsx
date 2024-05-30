import { BrowserRouter, Link, Route, Routes } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { NavMenu } from './components/navMenu/NavMenu';
import { Trainings } from './components/trainings/Trainings';
import { Mark } from './components/mark/Mark';
import { AppProvider } from '../src/api/contexts/appContext/AppContext'
import './App.sass';

function App() {
    return (
        <AppProvider>
            <BrowserRouter>
                <main className='page-container'>
                    <NavMenu />
                    <div className='page-container__element'>
                        <Routes>
                            <Route path="/" element={<Trainings />} />
                            <Route path="/mark" element={<Mark />} />
                        </Routes>
                    </div>
                </main>
            </BrowserRouter>
        </AppProvider>
    );
}

export default App;