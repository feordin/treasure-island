// src/components/SlideOutDrawer.tsx
import React from 'react';
import './SlideOutDrawer.css';

interface SlideOutDrawerProps {
  isOpen: boolean;
  onClose: () => void;
  position: 'left' | 'right';
  title: string;
  children: React.ReactNode;
}

const SlideOutDrawer: React.FC<SlideOutDrawerProps> = ({
  isOpen,
  onClose,
  position,
  title,
  children
}) => {
  return (
    <>
      {/* Overlay */}
      <div
        className={`drawer-overlay ${isOpen ? 'open' : ''}`}
        onClick={onClose}
      />

      {/* Drawer */}
      <div className={`slide-out-drawer ${position} ${isOpen ? 'open' : ''}`}>
        <div className="drawer-header">
          <h3>{title}</h3>
          <button className="drawer-close-btn" onClick={onClose}>
            &times;
          </button>
        </div>
        <div className="drawer-content">
          {children}
        </div>
      </div>
    </>
  );
};

export default SlideOutDrawer;
