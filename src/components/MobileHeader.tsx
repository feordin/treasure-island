// src/components/MobileHeader.tsx
import React from 'react';
import './MobileHeader.css';

interface MobileHeaderProps {
  onStatusClick: () => void;
  onSavedGamesClick: () => void;
  locationName?: string;
  isTtsEnabled?: boolean;
  onTtsToggle?: () => void;
  onTtsOff?: () => void;
  onStopSpeaking?: () => void;
  isSpeaking?: boolean;
  speechRateLabel?: string;
}

const MobileHeader: React.FC<MobileHeaderProps> = ({
  onStatusClick,
  onSavedGamesClick,
  locationName,
  isTtsEnabled = false,
  onTtsToggle,
  onTtsOff,
  onStopSpeaking,
  isSpeaking = false,
  speechRateLabel = '🔊'
}) => {
  return (
    <header className="mobile-header">
      <button className="header-btn" onClick={onStatusClick} title="Game Status">
        <span className="header-icon">&#9776;</span>
      </button>

      <div className="header-title">
        {locationName || 'Treasure Island'}
      </div>

      <div className="header-right">
        {onTtsToggle && (
          <>
            {isSpeaking && onStopSpeaking ? (
              <button
                className="header-btn tts-btn stop"
                onClick={onStopSpeaking}
                title="Stop speaking"
              >
                <span className="header-icon">⏹️</span>
              </button>
            ) : (
              <button
                className={`header-btn tts-btn ${isTtsEnabled ? 'active' : ''}`}
                onClick={onTtsToggle}
                title={isTtsEnabled ? 'Change speed (click) or turn off (hold)' : 'Enable voice'}
              >
                <span className="header-icon">{isTtsEnabled ? '🔊' : '🔇'}</span>
              </button>
            )}
            {isTtsEnabled && !isSpeaking && onTtsOff && (
              <button
                className="header-btn tts-off-btn"
                onClick={onTtsOff}
                title="Turn off voice"
              >
                <span className="header-icon">✕</span>
              </button>
            )}
          </>
        )}
        <button className="header-btn" onClick={onSavedGamesClick} title="Saved Games">
          <span className="header-icon">&#128190;</span>
        </button>
      </div>
    </header>
  );
};

export default MobileHeader;
