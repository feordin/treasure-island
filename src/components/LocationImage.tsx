// src/components/LocationImage.tsx
import React from 'react';
import './LocationImage.css';


interface LocationImageProps {
  imageSrc: string;
}

const LocationImage: React.FC<LocationImageProps> = ({ imageSrc }) => {
  console.log('LocationImage component re-rendered with imageSrc:', imageSrc);
  return (
    <div className="location-image">
      <img src={imageSrc} alt="Current Location" />
    </div>
  );
};

export default LocationImage;
