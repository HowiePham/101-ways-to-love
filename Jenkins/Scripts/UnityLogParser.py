#!/usr/bin/env python3
"""
Unity Build Log Parser - Extract and analyze build errors
"""

import re
from dataclasses import dataclass
from enum import Enum
from typing import List, Optional, Dict


class ErrorSeverity(Enum):
    CRITICAL = "critical"  # Build-blocking errors
    ERROR = "error"  # Compilation errors
    WARNING = "warning"  # Warnings
    INFO = "info"  # Information


@dataclass
class BuildError:
    severity: ErrorSeverity
    message: str
    file_path: Optional[str] = None
    line_number: Optional[int] = None
    column_number: Optional[int] = None
    error_code: Optional[str] = None
    category: Optional[str] = None

    def __str__(self):
        parts = [f"[{self.severity.value.upper()}]"]

        if self.category:
            parts.append(f"({self.category})")

        if self.file_path:
            location = self.file_path
            if self.line_number:
                location += f":{self.line_number}"
                if self.column_number:
                    location += f":{self.column_number}"
            parts.append(location)

        if self.error_code:
            parts.append(f"[{self.error_code}]")

        parts.append(self.message)

        return " ".join(parts)


class UnityLogParser:
    """Parse Unity build logs and extract errors"""

    # Regex patterns for different error types
    PATTERNS = {
        # C# compilation errors: Assets/Game/Scripts/UI/ErrorScript.cs(8,15): error CS0246: ...
        'csharp_error': re.compile(
            r'(?P<path>Assets[\\/][^()]+?\.cs)\('  # Assets/...path.../File.cs(
            r'(?P<line>\d+),(?P<col>\d+)\):\s*'  # (8,15):
            r'error\s+'  # error
            r'(?P<code>CS\d+):\s*'  # CS0246:
            r'(?P<message>.+)',  # lỗi chi tiết
            re.IGNORECASE
        ),

        # Unity errors: Error building Player: ...
        'unity_error': re.compile(
            r'Error building Player:\s*(?P<message>.+)'
        ),

        # General errors: Error: ...
        'general_error': re.compile(
            r'^Error:\s*(?P<message>.+)',
            re.IGNORECASE
        ),

        # Exception errors
        'exception': re.compile(
            r'(?P<type>\w+Exception):\s*(?P<message>.+)'
        ),

        # Build failed message
        'build_failed': re.compile(
            r'Build failed with (?P<count>\d+) error'
        ),

        # Android build errors
        'android_error': re.compile(
            r'CommandInvokationFailure:\s*(?P<message>.+)'
        ),

        # IL2CPP errors
        'il2cpp_error': re.compile(
            r'IL2CPP error.*?:\s*(?P<message>.+)'
        ),

        # Shader errors
        'shader_error': re.compile(
            r'Shader error in [\'"](?P<shader>[^\'"]+)[\'"]:\s*(?P<message>.+)'
        ),

        # Gradle build failure header
        'gradle_failure': re.compile(
            r'FAILURE: Build failed with an exception\.'
        ),

        # Gradle "What went wrong"
        'gradle_what_wrong': re.compile(
            r'\* What went wrong:'
        ),

        # Gradle task execution failed
        'gradle_task_failed': re.compile(
            r'Execution failed for task [\'"](?P<task>[^\'"]+)[\'"]'
        ),

        # Gradle AAPT errors
        'gradle_aapt_error': re.compile(
            r'AAPT:\s*error:\s*(?P<message>.+)'
        ),

        # Gradle Android resource linking failed
        'gradle_resource_error': re.compile(
            r'Android resource linking failed'
        ),

        # Gradle ERROR: prefix
        'gradle_error_line': re.compile(
            r'^\s*ERROR:\s*(?P<message>.+)'
        ),

        # Gradle task failed with exception (new)
        'gradle_task_exception': re.compile(
            r'Task failed with an exception\.'
        ),

        # Java IOException (disk space, etc)
        'java_io_exception': re.compile(
            r'>?\s*java\.io\.(?P<type>\w+):\s*(?P<message>.+)'
        ),

        # Disk space error
        'disk_space_error': re.compile(
            r'not enough space on the disk|No space left on device',
            re.IGNORECASE
        ),

        # Could not add entry error
        'cache_error': re.compile(
            r'Could not add entry [\'"](?P<entry>[^\'"]+)[\'"] to cache',
            re.IGNORECASE
        ),
        'native_crash_start': re.compile(
            r'={5,}\s*Native Crash Reporting\s*={5,}', re.IGNORECASE
        ),
        'native_crash_trace': re.compile(
            r'Got a (?P<type>\w+).*mono runtime|fatal error', re.IGNORECASE
        ),
    }

    def __init__(self):
        self.errors: List[BuildError] = []
        self.warnings: List[BuildError] = []
        self.stats = {
            'total_errors': 0,
            'total_warnings': 0,
            'categories': {}
        }

    def parse_file(self, log_file_path: str) -> Dict:
        """Parse Unity build log file"""
        try:
            with open(log_file_path, 'r', encoding='utf-8', errors='ignore') as f:
                content = f.read()

            return self.parse_content(content)
        except FileNotFoundError:
            print(f"Error: Log file not found: {log_file_path}")
            return None
        except Exception as e:
            print(f"Error reading log file: {e}")
            return None

    def parse_content(self, content: str) -> Dict:
        """Parse Unity build log content"""
        self.errors.clear()
        self.warnings.clear()

        # Dùng set để chống duplicate
        seen_errors = set()
        seen_warnings = set()

        lines = content.split('\n')
        i = 0
        while i < len(lines):
            line = lines[i].strip()
            if not line:
                i += 1
                continue

            if self.PATTERNS['native_crash_start'].search(line):
                error = self._parse_native_crash(lines, i)
                if error:
                    key = self._error_key(error)
                    if key not in seen_errors:
                        self.errors.append(error)
                        seen_errors.add(key)
                i += 40
                continue

            # Check for Gradle failure block
            if self.PATTERNS['gradle_failure'].search(line):
                error = self._parse_gradle_error_block(lines, i)
                if error:
                    key = self._error_key(error)
                    if key not in seen_errors:
                        self.errors.append(error)
                        seen_errors.add(key)
                i += 10
                continue

            # Try each error parser
            error = (
                    self._parse_csharp_error(line)
                    or self._parse_unity_error(line)
                    or self._parse_exception(line, lines, i)
                    or self._parse_shader_error(line)
                    or self._parse_il2cpp_error(line)
                    or self._parse_android_error(line)
                    or self._parse_gradle_aapt_error(line)
                    or self._parse_general_error(line)
            )

            if error:
                key = self._error_key(error)
                if error.severity == ErrorSeverity.ERROR:
                    if key not in seen_errors:
                        self.errors.append(error)
                        seen_errors.add(key)
                elif error.severity == ErrorSeverity.WARNING:
                    if key not in seen_warnings:
                        self.warnings.append(error)
                        seen_warnings.add(key)

            i += 1

        # Update statistics
        self._update_stats()

        return {
            'success': len(self.errors) == 0,
            'errors': self.errors,
            'warnings': self.warnings,
            'stats': self.stats,
            'summary': self._generate_summary()
        }

    def _error_key(self, error: BuildError) -> str:
        """
        Generate a unique hashable key for an error, to prevent duplicates.
        Combines category, file path, line, code, and message.
        """
        path = (error.file_path or "").lower()
        code = (error.error_code or "").lower()
        cat = (error.category or "").lower()
        msg = (error.message or "").strip().lower()

        return f"{cat}|{path}|{error.line_number}|{error.column_number}|{code}|{msg}"

    def _parse_native_crash(self, lines: List[str], start_index: int) -> Optional[BuildError]:
        """Detect 'Native Crash Reporting' section."""
        end_index = min(start_index + 50, len(lines))
        crash_summary = []
        crash_type = "Unknown"
        for i in range(start_index, end_index):
            line = lines[i].strip()
            if "=================================================================" in line and i > start_index:
                break
            crash_summary.append(line)
            match = self.PATTERNS['native_crash_trace'].search(line)
            if match:
                crash_type = match.group('type') or "Unknown"

        if not crash_summary:
            return None
        message = "\n".join(crash_summary[:15])
        return BuildError(
            severity=ErrorSeverity.CRITICAL,
            message=message.strip(),
            category=f"Native Crash ({crash_type})"
        )

    def _parse_csharp_error(self, line: str) -> Optional[BuildError]:
        """Parse C# compilation error (only errors, not warnings)"""
        match = self.PATTERNS['csharp_error'].search(line)
        if match:
            # Normalize path separators so Windows \ becomes /
            path = match.group('path').replace('\\', '/').strip()

            # Only return if it's a C# file in Assets folder
            if path.lower().startswith('assets/') and path.endswith('.cs'):
                return BuildError(
                    severity=ErrorSeverity.ERROR,
                    message=match.group('message').strip(),
                    file_path=path,
                    line_number=int(match.group('line')),
                    column_number=int(match.group('col')),
                    error_code=match.group('code'),
                    category='C# Compilation'
                )

        return None

    def _parse_unity_error(self, line: str) -> Optional[BuildError]:
        """Parse Unity build error"""
        match = self.PATTERNS['unity_error'].search(line)
        if match:
            return BuildError(
                severity=ErrorSeverity.CRITICAL,
                message=match.group('message').strip(),
                category='Unity Build'
            )
        return None

    def _parse_exception(self, line: str, lines: List[str], index: int) -> Optional[BuildError]:
        """Parse exception with stack trace"""
        match = self.PATTERNS['exception'].search(line)
        if match:
            # Try to get stack trace
            stack_trace = []
            for i in range(index + 1, min(index + 5, len(lines))):
                if lines[i].strip().startswith('at '):
                    stack_trace.append(lines[i].strip())
                else:
                    break

            message = match.group('message').strip()
            if stack_trace:
                message += '\n' + '\n'.join(stack_trace[:3])  # First 3 lines

            return BuildError(
                severity=ErrorSeverity.CRITICAL,
                message=message,
                category=match.group('type')
            )
        return None

    def _parse_shader_error(self, line: str) -> Optional[BuildError]:
        """Parse shader compilation error"""
        match = self.PATTERNS['shader_error'].search(line)
        if match:
            return BuildError(
                severity=ErrorSeverity.ERROR,
                message=match.group('message').strip(),
                file_path=match.group('shader'),
                category='Shader'
            )
        return None

    def _parse_il2cpp_error(self, line: str) -> Optional[BuildError]:
        """Parse IL2CPP error"""
        match = self.PATTERNS['il2cpp_error'].search(line)
        if match:
            return BuildError(
                severity=ErrorSeverity.CRITICAL,
                message=match.group('message').strip(),
                category='IL2CPP'
            )
        return None

    def _parse_android_error(self, line: str) -> Optional[BuildError]:
        """Parse Android build error"""
        match = self.PATTERNS['android_error'].search(line)
        if match:
            return BuildError(
                severity=ErrorSeverity.CRITICAL,
                message=match.group('message').strip(),
                category='Android Build'
            )
        return None

    def _parse_general_error(self, line: str) -> Optional[BuildError]:
        """Parse general error"""
        match = self.PATTERNS['general_error'].search(line)
        if match:
            message = match.group('message').strip()
            # Skip false positives
            if any(skip in message.lower() for skip in ['error code', 'no error', 'without error']):
                return None

            return BuildError(
                severity=ErrorSeverity.ERROR,
                message=message,
                category='General'
            )
        return None

    def _parse_gradle_error_block(self, lines: List[str], start_index: int) -> Optional[BuildError]:
        """Parse Gradle error block"""
        error_details = []
        task_name = None
        main_error = None
        aapt_errors = []
        java_exceptions = []
        is_disk_space_error = False
        cache_entry = None

        # Look ahead up to 40 lines to capture the full error
        end_index = min(start_index + 40, len(lines))

        for i in range(start_index, end_index):
            line = lines[i].strip()

            # Check for disk space error
            if self.PATTERNS['disk_space_error'].search(line):
                is_disk_space_error = True

            # Check for cache error
            cache_match = self.PATTERNS['cache_error'].search(line)
            if cache_match:
                cache_entry = cache_match.group('entry')

            # Get task name
            if not task_name:
                task_match = self.PATTERNS['gradle_task_failed'].search(line)
                if task_match:
                    task_name = task_match.group('task')

            # Get main error message
            if self.PATTERNS['gradle_what_wrong'].search(line):
                # Next few lines usually have the error
                for j in range(i + 1, min(i + 5, len(lines))):
                    next_line = lines[j].strip()
                    if next_line and not next_line.startswith('*') and not next_line.startswith('>'):
                        main_error = next_line
                        break

            # Get Java IO exceptions
            java_ex_match = self.PATTERNS['java_io_exception'].search(line)
            if java_ex_match:
                java_exceptions.append(f"{java_ex_match.group('type')}: {java_ex_match.group('message').strip()}")

            # Get AAPT errors (most specific)
            aapt_match = self.PATTERNS['gradle_aapt_error'].search(line)
            if aapt_match:
                aapt_errors.append(aapt_match.group('message').strip())

            # Get ERROR: lines
            error_match = self.PATTERNS['gradle_error_line'].search(line)
            if error_match:
                error_details.append(error_match.group('message').strip())

            # Resource linking failed
            if self.PATTERNS['gradle_resource_error'].search(line):
                if not main_error:
                    main_error = "Android resource linking failed"

            # Stop at "* Try:" or "BUILD FAILED"
            if line.startswith('* Try:') or 'BUILD FAILED' in line:
                break

        # Build error message with priority
        message_parts = []
        category = 'Gradle Build'

        # Priority 1: Disk space error
        if is_disk_space_error:
            category = 'Disk Space'
            message_parts.append("❗ NOT ENOUGH DISK SPACE")
            if cache_entry:
                message_parts.append(f"Failed to write: {cache_entry}")
            if java_exceptions:
                message_parts.extend(java_exceptions[:1])

        # Priority 2: Java exceptions (IO errors, etc)
        elif java_exceptions:
            category = 'System Error'
            message_parts.append(f"Task: {task_name or 'Unknown'}")
            message_parts.extend(java_exceptions[:2])

        # Priority 3: AAPT errors
        elif aapt_errors:
            category = 'Android Resources'
            message_parts.append(f"Task: {task_name or 'Unknown'}")
            message_parts.extend(aapt_errors[:2])

        # Priority 4: Task execution errors
        elif error_details:
            message_parts.append(f"Task: {task_name or 'Unknown'}")
            if main_error:
                message_parts.append(main_error)
            message_parts.extend(error_details[:2])

        # Priority 5: Generic task failure
        elif task_name:
            message_parts.append(f"Task: {task_name}")
            if main_error:
                message_parts.append(main_error)

        # Default
        else:
            message_parts.append("Gradle Build Failed - See build log for details")

        message = '\n'.join(message_parts)

        # Extract file path from AAPT error if available
        file_path = None
        for error_line in error_details + aapt_errors:
            # Look for file paths like: E:\...\AndroidManifest.xml:83:13-65:
            path_match = re.search(r'([A-Z]:\\[^:]+|/[^:]+):\d+:', error_line)
            if path_match:
                file_path = path_match.group(1)
                break

        return BuildError(
            severity=ErrorSeverity.CRITICAL,
            message=message,
            file_path=file_path,
            category=category,
            error_code=task_name
        )

    def _parse_gradle_aapt_error(self, line: str) -> Optional[BuildError]:
        """Parse standalone AAPT error"""
        match = self.PATTERNS['gradle_aapt_error'].search(line)
        if match:
            message = match.group('message').strip()

            # Extract file path if present
            file_path = None
            path_match = re.search(r'([A-Z]:\\[^:]+|/[^:]+):\d+:', message)
            if path_match:
                file_path = path_match.group(1)

            return BuildError(
                severity=ErrorSeverity.ERROR,
                message=message,
                file_path=file_path,
                category='Android Resources'
            )
        return None

    def _update_stats(self):
        """Update statistics"""
        self.stats['total_errors'] = len(self.errors)
        self.stats['total_warnings'] = len(self.warnings)
        self.stats['categories'] = {}

        for error in self.errors + self.warnings:
            category = error.category or 'Unknown'
            self.stats['categories'][category] = self.stats['categories'].get(category, 0) + 1

    def _generate_summary(self) -> str:
        """Generate error summary"""
        if len(self.errors) == 0:
            return "✅ Build succeeded with no errors"

        lines = [
            f"❌ Build failed with {len(self.errors)} error(s) and {len(self.warnings)} warning(s)",
            "",
            "Error breakdown by category:"
        ]

        for category, count in sorted(self.stats['categories'].items(), key=lambda x: -x[1]):
            lines.append(f"  - {category}: {count}")

        return "\n".join(lines)

    def generate_detail_report(self) -> str:
        """Generate detailed Slack-friendly build report message (full errors)"""
        lines = []

        # ===== Separate Critical vs Regular =====
        critical_errors = [e for e in self.errors if e.severity == ErrorSeverity.CRITICAL]
        regular_errors = [e for e in self.errors if e.severity == ErrorSeverity.ERROR]

        # ===== Critical =====
        if critical_errors:
            lines.append("")
            for e in critical_errors:
                msg = e.message.replace("\n", " ").strip()
                lines.append(f"🔴 *{e.category or 'General'}* → {msg[:300]}{'...' if len(msg) > 300 else ''}")

        # ===== Regular =====
        if regular_errors:
            lines.append("")
            from collections import defaultdict
            grouped = defaultdict(list)
            for e in regular_errors:
                grouped[e.category or 'General'].append(e)

            for category, errs in grouped.items():
                if category == "C# Compilation":
                    lines.append(f"\n❌ *C# Compilation Errors ({len(errs)})*")
                    for e in errs:
                        path = e.file_path or "Unknown file"
                        line = f"{e.line_number}" if e.line_number else "?"
                        col = f"{e.column_number}" if e.column_number else "?"
                        code = f"[{e.error_code}]" if e.error_code else ""
                        msg = e.message.strip().replace("\n", " ")
                        # Code block for clarity
                        lines.append("```")
                        lines.append(f"File: {path}")
                        lines.append(f"Line: {line}, Column: {col}")
                        if code:
                            lines.append(f"Code: {code}")
                        lines.append(f"Message: {msg}")
                        lines.append("```")
                else:
                    lines.append(f"\n❌ *{category} ({len(errs)})*")
                    for e in errs:
                        msg = e.message.strip().replace("\n", " ")
                        lines.append(f"• {msg[:300]}{'...' if len(msg) > 300 else ''}")

        lines.append("")
        return "\n".join(lines)

    def print_report(self, verbose: bool = False):
        """Print error report"""
        print("=" * 80)
        print("UNITY BUILD LOG ANALYSIS")
        print("=" * 80)
        print()

        # Summary
        print(self._generate_summary())
        print()

        # Critical errors first
        critical_errors = [e for e in self.errors if e.severity == ErrorSeverity.CRITICAL]
        if critical_errors:
            print("CRITICAL ERRORS:")
            print("-" * 80)
            for error in critical_errors:
                print(f"  {error}")
            print()

        # Regular errors
        regular_errors = [e for e in self.errors if e.severity == ErrorSeverity.ERROR]
        if regular_errors:
            print("ERRORS:")
            print("-" * 80)
            for error in regular_errors[:10]:  # Show first 10
                print(f"  {error}")

            if len(regular_errors) > 10:
                print(f"  ... and {len(regular_errors) - 10} more errors")
            print()

        # Warnings (if verbose)
        if verbose and self.warnings:
            print("WARNINGS:")
            print("-" * 80)
            for warning in self.warnings[:5]:  # Show first 5
                print(f"  {warning}")

            if len(self.warnings) > 5:
                print(f"  ... and {len(self.warnings) - 5} more warnings")
            print()

        print("=" * 80)

    def get_most_common_errors(self, top_n: int = 5) -> List[Dict]:
        """Get most common error patterns"""
        error_counts = {}

        for error in self.errors:
            # Group by error code or category
            key = error.error_code or error.category or 'Unknown'
            if key not in error_counts:
                error_counts[key] = {'count': 0, 'example': error}
            error_counts[key]['count'] += 1

        # Sort by count
        sorted_errors = sorted(error_counts.items(), key=lambda x: -x[1]['count'])

        return [
            {
                'type': key,
                'count': value['count'],
                'example': str(value['example'])
            }
            for key, value in sorted_errors[:top_n]
        ]

    def export_to_json(self, output_file: str):
        """Export errors to JSON"""
        import json

        data = {
            'success': len(self.errors) == 0,
            'errors': [
                {
                    'severity': e.severity.value,
                    'message': e.message,
                    'file': e.file_path,
                    'line': e.line_number,
                    'column': e.column_number,
                    'code': e.error_code,
                    'category': e.category
                }
                for e in self.errors
            ],
            'warnings': [
                {
                    'severity': w.severity.value,
                    'message': w.message,
                    'file': w.file_path,
                    'line': w.line_number,
                    'code': w.error_code,
                    'category': w.category
                }
                for w in self.warnings
            ],
            'stats': self.stats,
            'top_errors': self.get_most_common_errors()
        }

        with open(output_file, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=2, ensure_ascii=False)

        print(f"Exported to: {output_file}")
