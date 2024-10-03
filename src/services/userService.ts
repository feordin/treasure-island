// userService.ts

// Define the User interface
export interface User {
  userId: string;
  userDetails: string;
  userRoles: string[];
}

// Implement the method to get user information
export const getUserInfo = async (): Promise<User | null> => {
  try {
    const response = await fetch('/.auth/me');
    if (!response.ok) {
      throw new Error('Network response was not ok');
    }
    const payload = await response.json();
    const { clientPrincipal } = payload;
    return clientPrincipal as User;
  } catch (error) {
    console.error('Failed to fetch user info:', error);
    return null;
  }
};