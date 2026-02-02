export interface TestUser {
  userId: string;
  userName: string;
  userRoles: string[];
  identityProvider: string;
  userDetails: string;
}

export function generateTestUserId(): string {
  const timestamp = Date.now();
  const random = Math.random().toString(36).substring(2, 8);
  return `test_user_${timestamp}_${random}`;
}

export function generateRunId(): string {
  const now = new Date();
  const date = now.toISOString().split('T')[0];
  const time = now.toTimeString().split(' ')[0].replace(/:/g, '');
  const random = Math.random().toString(36).substring(2, 6);
  return `run_${date}_${time}_${random}`;
}

export function createTestUser(): TestUser {
  return {
    userId: generateTestUserId(),
    userName: 'API Test User',
    userRoles: ['authenticated'],
    identityProvider: 'test',
    userDetails: 'automated-test'
  };
}
