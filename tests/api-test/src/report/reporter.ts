import { writeFileSync, mkdirSync, existsSync } from 'fs';
import { TestReport, TestSuite, TestResult } from '../types/gameTypes.js';

export class Reporter {
  private outputDir: string;

  constructor(outputDir: string = './output') {
    this.outputDir = outputDir;
    if (!existsSync(outputDir)) {
      mkdirSync(outputDir, { recursive: true });
    }
  }

  generateReport(report: TestReport): void {
    this.printConsoleReport(report);
    this.saveJsonReport(report);
    this.saveMarkdownReport(report);
  }

  private printConsoleReport(report: TestReport): void {
    console.log('\n' + '='.repeat(60));
    console.log('TEST REPORT');
    console.log('='.repeat(60));
    console.log(`Run ID: ${report.runId}`);
    console.log(`API URL: ${report.apiUrl}`);
    console.log(`Duration: ${(report.endTime.getTime() - report.startTime.getTime()) / 1000}s`);
    console.log('-'.repeat(60));
    console.log(`Total: ${report.summary.totalTests}`);
    console.log(`Passed: ${report.summary.passed}`);
    console.log(`Failed: ${report.summary.failed}`);
    console.log(`Skipped: ${report.summary.skipped}`);
    console.log('-'.repeat(60));

    for (const suite of report.suites) {
      console.log(`\n[${suite.name}] ${suite.passed}/${suite.tests.length} passed`);
      for (const test of suite.tests) {
        const icon = test.passed ? '✓' : '✗';
        console.log(`  ${icon} ${test.name} (${test.duration}ms)`);
        if (!test.passed && test.error) {
          console.log(`    Error: ${test.error}`);
        }
      }
    }

    console.log('\n' + '='.repeat(60));
    const status = report.summary.failed === 0 ? 'ALL TESTS PASSED' : 'SOME TESTS FAILED';
    console.log(status);
    console.log('='.repeat(60) + '\n');
  }

  private saveJsonReport(report: TestReport): void {
    const filename = `${this.outputDir}/report-${report.runId}.json`;
    writeFileSync(filename, JSON.stringify(report, null, 2));
    console.log(`JSON report saved: ${filename}`);
  }

  private saveMarkdownReport(report: TestReport): void {
    const filename = `${this.outputDir}/report-${report.runId}.md`;
    const md = this.generateMarkdown(report);
    writeFileSync(filename, md);
    console.log(`Markdown report saved: ${filename}`);
  }

  private generateMarkdown(report: TestReport): string {
    let md = `# Test Report: ${report.runId}\n\n`;
    md += `**API URL:** ${report.apiUrl}\n\n`;
    md += `**Duration:** ${(report.endTime.getTime() - report.startTime.getTime()) / 1000}s\n\n`;
    md += `## Summary\n\n`;
    md += `| Metric | Count |\n|--------|-------|\n`;
    md += `| Total | ${report.summary.totalTests} |\n`;
    md += `| Passed | ${report.summary.passed} |\n`;
    md += `| Failed | ${report.summary.failed} |\n`;
    md += `| Skipped | ${report.summary.skipped} |\n\n`;

    for (const suite of report.suites) {
      md += `## ${suite.name}\n\n`;
      md += `| Test | Status | Duration |\n|------|--------|----------|\n`;
      for (const test of suite.tests) {
        const status = test.passed ? '✅ Pass' : '❌ Fail';
        md += `| ${test.name} | ${status} | ${test.duration}ms |\n`;
      }
      md += '\n';
    }

    return md;
  }
}

export function createTestResult(
  name: string,
  category: string,
  passed: boolean,
  duration: number,
  error?: string,
  details?: Record<string, unknown>
): TestResult {
  return { name, category, passed, duration, error, details };
}

export function createTestSuite(name: string, tests: TestResult[]): TestSuite {
  return {
    name,
    tests,
    passed: tests.filter(t => t.passed).length,
    failed: tests.filter(t => !t.passed).length,
    skipped: 0
  };
}
