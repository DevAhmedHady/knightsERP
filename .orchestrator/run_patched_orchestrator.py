import json
import os
import re
import subprocess
import sys
from pathlib import Path


REPO_ROOT = Path(__file__).resolve().parents[1]
SKILL_DIR = Path(r"C:\Users\AhmedHady\.codex\skills\multi-model-orchestrator")
LOG_DIR = REPO_ROOT / ".orchestrator"
LOG_DIR.mkdir(parents=True, exist_ok=True)

sys.path.insert(0, str(SKILL_DIR))

import adapters  # noqa: E402
import orchestrator  # noqa: E402


def _write_log(name: str, content: str) -> None:
    (LOG_DIR / name).write_text(content, encoding="utf-8")


def _patched_claude_cli_call(model, system, user, max_tokens, reasoning_effort, cwd=None, **_):
    claude_exe = adapters._find_exe("claude")
    if claude_exe is None:
        raise RuntimeError("claude CLI not found on PATH.")

    cmd = [
        claude_exe,
        "-p",
        "--model",
        model,
        "--system-prompt",
        system,
        "--output-format",
        "json",
        "--disallowedTools",
        "Bash,Edit,Write,NotebookEdit",
    ]
    if reasoning_effort:
        cmd += ["--effort", adapters._CLAUDE_EFFORT.get(reasoning_effort, "medium")]

    proc = subprocess.run(
        cmd,
        cwd=cwd,
        capture_output=True,
        text=True,
        timeout=900,
        input=user,
    )

    _write_log("last-claude-stdout.json", proc.stdout)
    _write_log("last-claude-stderr.txt", proc.stderr)

    if proc.returncode != 0:
        raise RuntimeError(f"claude -p failed ({proc.returncode}):\n{proc.stderr[-1500:]}")

    try:
        env = json.loads(proc.stdout)
        text = env.get("result", proc.stdout)
        usage = {
            "in": (env.get("usage") or {}).get("input_tokens", 0),
            "out": (env.get("usage") or {}).get("output_tokens", 0),
        }
    except json.JSONDecodeError:
        text = proc.stdout.strip()
        usage = {"in": 0, "out": 0}

    _write_log("last-claude-result.txt", text)
    return adapters.LLMResult(text=text, usage=usage)


def _patched_codex_cli_call(model, system, user, max_tokens, reasoning_effort, cwd=None, sandbox="read-only", **_):
    codex_exe = adapters._find_exe("codex")
    if codex_exe is None:
        raise RuntimeError("codex CLI not found on PATH.")

    prompt = f"{system}\n\n---\n\n{user}"
    cmd = [
        codex_exe,
        "exec",
        "-c",
        f'model="{model}"',
        "-c",
        f'model_reasoning_effort="{reasoning_effort or "medium"}"',
        "--sandbox",
        sandbox,
        "--ephemeral",
        "--skip-git-repo-check",
        prompt,
    ]

    proc = subprocess.run(
        cmd,
        cwd=cwd,
        capture_output=True,
        timeout=900,
        encoding="utf-8",
        errors="replace",
    )

    stderr = proc.stderr or ""
    stdout = proc.stdout or ""

    _write_log("last-codex-stdout.txt", stdout)
    _write_log("last-codex-stderr.txt", stderr)

    if proc.returncode != 0:
        raise RuntimeError(f"codex exec failed ({proc.returncode}):\n{stderr[-1500:]}")

    text = stdout.strip()
    m = re.search(r"tokens used\s+([\d,]+)", stderr)
    total = int(m.group(1).replace(",", "")) if m else 0
    _write_log("last-codex-result.txt", text)
    return adapters.LLMResult(text=text, usage={"in": 0, "out": total})


def _patched_extract_json(text: str) -> dict:
    raw = text.strip()
    raw = re.sub(r"^```(?:json)?|```$", "", raw, flags=re.MULTILINE).strip()

    candidates = [raw]

    try:
        outer = json.loads(raw)
        if isinstance(outer, dict) and "result" in outer and isinstance(outer["result"], str):
            candidates.insert(0, outer["result"].strip())
    except Exception:
        pass

    for candidate in candidates:
        try:
            return json.loads(candidate)
        except json.JSONDecodeError:
            start = candidate.find("{")
            if start >= 0:
                depth = 0
                for i in range(start, len(candidate)):
                    depth += (candidate[i] == "{") - (candidate[i] == "}")
                    if depth == 0:
                        snippet = candidate[start:i + 1]
                        try:
                            return json.loads(snippet)
                        except json.JSONDecodeError:
                            continue

    _write_log("last-json-parse-failure.txt", text)
    raise ValueError("No valid JSON object found in model output. See .orchestrator logs.")


adapters._DISPATCH["claude_cli"] = _patched_claude_cli_call
adapters._DISPATCH["codex_cli"] = _patched_codex_cli_call
orchestrator.extract_json = _patched_extract_json


if __name__ == "__main__":
    orchestrator.main(sys.argv[1:])
