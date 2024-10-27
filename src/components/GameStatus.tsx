import React from 'react';
import './GameStatus.css';
import { getUserInfo, User } from '../services/userService';


interface GameStatusProps {
  inventory: string[] | undefined;
  score: number | undefined;
  location: string | undefined;
  date: string | undefined;
  time: string | undefined;
  currentLocationDescription: string | undefined;
  AiEmbelleshedDescriptions: boolean;
  money: number | undefined;
  facing: string | undefined
}

const GameStatus: React.FC<GameStatusProps> = ({ inventory, score, location, date, time, currentLocationDescription, AiEmbelleshedDescriptions, money, facing }) => {
    const [user, setUser] = React.useState<User | null>(null);

    React.useEffect(() => {
        const fetchUserInfo = async () => {
            const userInfo = await getUserInfo();
            setUser(userInfo);
        };
        fetchUserInfo();
    }, []);

  return (
    <div className="game-status">
      <h2>Game Status</h2>
      <div className="status-item">
        <strong>Player:</strong> {user?.userDetails}
      </div>
      <div className="status-item">
        <strong>Score:</strong> {score}
      </div>
      <div className="status-item">
        <strong>Date:</strong> {date}
      </div>
      <div className="status-item">
        <strong>Time:</strong> {time}
      </div>
      <div className="status-item">
        <strong>Currently facing:</strong> {facing}
      </div>
      <div className="status-item">
        <strong>Money:</strong> {money}
      </div>
      <div className="status-item">
        <strong>Use AI to embellish the location descriptions:</strong> {AiEmbelleshedDescriptions ? "True" : "False"}
      </div>
      <div className="inventory">
        <strong>Inventory:</strong>
        <div className="inventory-list-container">
            <ul>
                {(inventory || []).map((item, index) => (
                <li key={index}>{item}</li>
                ))}
            </ul>
        </div>
      </div>
      <div className="status-item">
        <strong>Location:</strong> {location}
      </div>
      <div className="status-item">
        <strong>Location Description:</strong> {currentLocationDescription} {/* New text block */}
      </div>
    </div>
  );
};

export default GameStatus;