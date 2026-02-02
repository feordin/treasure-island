import { ApiClient } from './client/apiClient.js';
import { LocationGraph } from './utils/locationGraph.js';
import { createTestUser } from './utils/testUser.js';
import { Reporter, createTestSuite } from './report/reporter.js';
import { TestReport, TestResult } from './types/gameTypes.js';

// Test categories
import { runLocationTests } from './tests/locationTests.js';
import { runMechanicsTests } from './tests/mechanicsTests.js';
import { runItemTests } from './tests/itemTests.js';

interface TestSuite {
  name: string;
  category: string;
  run: (client: ApiClient, graph: LocationGraph) => Promise<void>;
}

const testSuites: TestSuite[] = [
  { name: 'Location Navigation', category: 'locations', run: runLocationTests },
  { name: 'Game Mechanics', category: 'mechanics', run: runMechanicsTests },
  { name: 'Item Interactions', category: 'items', run: runItemTests }
];

async function main() {
  const args = process.argv.slice(2);
  const categoryFilter = args.find(arg => arg.startsWith('--category='))?.split('=')[1];

  console.log('🏝️  Treasure Island API Test Suite\n');

  // Create test user
  const testUser = createTestUser();
  console.log(`Test User: ${testUser.userId} (${testUser.userName})`);

  // Initialize API client
  const apiEndpoint = process.env.API_ENDPOINT || 'http://localhost:7071/api';
  const client = new ApiClient(apiEndpoint, testUser);
  console.log(`API Endpoint: ${apiEndpoint}\n`);

  // Load world data and build location graph
  console.log('Loading world data...');
  await client.initialize();
  const graph = new LocationGraph(client.getWorldData());
  console.log(`✓ Loaded ${graph.getLocationCount()} locations\n`);

  // Filter test suites
  const suitesToRun = categoryFilter
    ? testSuites.filter(suite => suite.category === categoryFilter)
    : testSuites;

  if (suitesToRun.length === 0) {
    console.error(`❌ No tests found for category: ${categoryFilter}`);
    process.exit(1);
  }

  // Initialize reporter
  const reporter = new Reporter('./output');
  const startTime = new Date();
  const runId = `run-${startTime.toISOString().replace(/[:.]/g, '-')}`;
  const reportSuites: import('./types/gameTypes.js').TestSuite[] = [];

  // Run test suites
  let totalPassed = 0;
  let totalFailed = 0;

  for (const suite of suitesToRun) {
    console.log(`\n${'='.repeat(60)}`);
    console.log(`Running: ${suite.name}`);
    console.log('='.repeat(60));

    const suiteStartTime = Date.now();
    let suitePassed = false;
    let suiteError: string | undefined;

    try {
      await suite.run(client, graph);
      suitePassed = true;
      totalPassed++;
      console.log(`✓ ${suite.name} PASSED`);
    } catch (error) {
      suitePassed = false;
      suiteError = error instanceof Error ? error.message : String(error);
      totalFailed++;
      console.error(`✗ ${suite.name} FAILED`);
      console.error(error);
    }

    const suiteDuration = Date.now() - suiteStartTime;

    // Create test result for this suite
    const testResult: TestResult = {
      name: suite.name,
      category: suite.category,
      passed: suitePassed,
      duration: suiteDuration,
      error: suiteError
    };

    // Add to report
    reportSuites.push(createTestSuite(suite.name, [testResult]));

    // Reset game state between suites
    await client.sendCommand('startup');
  }

  const endTime = new Date();

  // Build final report
  const report: TestReport = {
    runId,
    startTime,
    endTime,
    apiUrl: apiEndpoint,
    summary: {
      totalTests: suitesToRun.length,
      passed: totalPassed,
      failed: totalFailed,
      skipped: 0
    },
    suites: reportSuites
  };

  // Generate reports
  reporter.generateReport(report);

  process.exit(totalFailed > 0 ? 1 : 0);
}

main().catch(error => {
  console.error('Fatal error:', error);
  process.exit(1);
});
