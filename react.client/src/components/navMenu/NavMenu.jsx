import React, { Component } from 'react';
import { Link } from 'react-router-dom';
import './NavMenu.sass'

export const NavMenu = () => {
    return (
        <>
            <nav>
                <ul className="navbar">
                    <Link className='navbar__element' to='/'>
                        <li>Тренировки</li>
                    </Link>
                    <Link className='navbar__element' to='/mark'>
                        <li>Оценка</li>
                    </Link>
                    <Link className='navbar__element' to='/graphic'>
                        <li>График</li>
                    </Link>
                </ul>
            </nav>
        </>
    );
}