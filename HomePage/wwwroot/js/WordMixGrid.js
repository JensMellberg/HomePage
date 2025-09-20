
class WordSquare {
    constructor(x, y, element) {
        this.x = x;
        this.y = y;
        this.element = element;
    }
}

class Letter {
    constructor(x, y, points, letter, element) {
        this.x = x;
        this.y = y;
        this.points = points
        this.letter = letter
        this.element = element
    }

    bindEvents() {
        window.addEventListener("beforeunload", function (e) {
            e.preventDefault();
            e.returnValue = "Säker? Du kanske inte kan komma tillbaka till sidan.";
        });

        this.element.addEventListener("touchstart", (e) => {
            const startX = parseInt(this.element.style.left.replace('px', ''))
            const startY = parseInt(this.element.style.top.replace('px', ''))
            const mouseStartX = e.touches[0].clientX
            const mouseStartY = e.touches[0].clientY
            const theElement = this.element
            const _this = this

            function removePrevious() {
                if (_this.x !== null) {
                    squares[_this.x][_this.y].letter = null
                    _this.x = null
                    _this.y = null
                } else {
                    storage.removeLetter(_this)
                }
            }

            function onMove(e) {
                theElement.style.left = startX + (e.touches[0].clientX - mouseStartX) + "px";
                theElement.style.top = startY + (e.touches[0].clientY - mouseStartY) + "px";
                e.preventDefault()
                return false
            }

            function onUp() {
                document.removeEventListener("touchmove", onMove);
                document.removeEventListener("touchend", onUp);
                const elementTop = theElement.getBoundingClientRect().top + squareSize / 2
                const elementLeft = theElement.getBoundingClientRect().left + squareSize / 2
                let foundMatch = false
                $('.word-mix-invalid').removeClass('word-mix-invalid')
                $('.word-mix-square').each((_, e) => {
                    const rect = e.getBoundingClientRect()
                    if (elementTop > rect.top && elementTop <= rect.top + e.clientHeight
                        && elementLeft > rect.left && elementLeft <= rect.left + e.clientWidth) {
                        foundMatch = true
                        if (e.square.letter) {
                            theElement.style.left = startX + 'px'
                            theElement.style.top = startY + 'px'
                            return false
                        }

                        removePrevious()
                        e.square.letter = _this
                        _this.x = e.square.x
                        _this.y = e.square.y
                        theElement.style.left = rect.left + 'px'
                        theElement.style.top = rect.top + 'px'
                        updateStorage()

                        return false
                        }
                })

                if (!foundMatch) {
                    removePrevious()
                    storage.addLetter(_this)
                    storage.rePosition()
                    updateStorage()
                }

                return false
            }

            document.addEventListener("touchmove", onMove, { passive: false });
            document.addEventListener("touchend", onUp);
            e.preventDefault()
            return false
        }, { passive: false });
    }
}

function updateStorage() {
    localStorage.setItem("savedboard", BoardToString(squares));
}

function getFromStorage() {
    const savedString = localStorage.getItem("savedboard")
    return savedString
}

class Queue {
    constructor() {
        this.items = {};
        this.head = 0;
        this.tail = 0;
    }

    enqueue(element) {
        this.items[this.tail] = element;
        this.tail++;
    }

    dequeue() {
        if (this.isEmpty()) {
            return undefined;
        }

        const item = this.items[this.head];
        delete this.items[this.head];
        this.head++;
        return item;
    }

    isEmpty() {
        return this.head === this.tail;
    }
}

class Storage {
    constructor() {
        this.create()
    }

    create() {
        this.element = document.createElement('div')
        this.element.className = 'word-mix-storage'
        this.letters = []
    }

    addLetter(letter) {
        letter.element.classList.add('storage-letter')
        letter.element.classList.remove('grid-letter')
        this.letters.push(letter)
    }

    removeLetter(letter) {
        this.letters = this.letters.filter(x => x !== letter)
    }

    rePosition() {
        let y = this.element.getBoundingClientRect().top + 10
        for (let i = 0; i < this.letters.length; i++) {
            if (i === 7) {
                y += 10 + squareSize
            }

            this.letters[i].element.style.left = (10 * ((i % 7) + 1) + (i % 7) * squareSize) + 'px'
            this.letters[i].element.style.top = y + 'px'
        }
    }
}

var squareSize = 0
var squares = []
var recordCallback = (score, board) => true

function drawGrid(startX, endX, startY, wrapper) {
    const gridWidth = 10
    const gridHeight = 10
    squares = new Array(gridWidth);
    for (let i = 0; i < gridWidth; i++) {
        squares[i] = new Array(gridHeight).fill(undefined);
    }

    squareSize = (endX - startX) / gridWidth
    for (let i = 0; i < gridWidth; i++) {
        for (let f = 0; f < gridHeight; f++) {
            const square = document.createElement('div')
            square.classList = 'word-mix-square'
            wrapper.appendChild(square)
            square.style.left = startX + (i * squareSize) + 'px'
            square.style.top = startY + (f * squareSize) + 'px'
            square.style.width = squareSize + 'px'
            square.style.height = squareSize + 'px'
            const squareObject = new WordSquare(i, f, square)
            square.square = squareObject
            squares[i][f] = squareObject
        }
    }
}

function BoardToString(board) {
    let result = ''
    for (let i = 0; i < board.length; i++) {
        for (let f = 0; f < board[0].length; f++) {
            const letter = board[i][f].letter
            if (letter) {
                result += i + ',' + f + ',' + letter.points + ',' + letter.letter + ';'
            }
        }
    }

    for (let i = 0; i < storage.letters.length; i++) {
        const letter = storage.letters[i]
        result += '-,-,' + letter.points + ',' + letter.letter + ';'
    }

    return result
}

function addWords(words) {
    for (const word of decodeForClient(words).split(',')) {
        dict[word.toLowerCase()] = true
    }
}

function StringToBoard(encoded) {
    const result = []
    const storageLetters = []
    const entries = encoded.split(';')
    for (let i = 0; i < entries.length - 1; i++) {
        const tokens = entries[i].split(',')
        if (tokens[0] === '-') {
            storageLetters.push(new Letter(null, null, parseInt(tokens[2]), tokens[3]))
        } else {
            result.push(new Letter(parseInt(tokens[0]), parseInt(tokens[1]), parseInt(tokens[2]), tokens[3]))
        }
    }

    return [result, storageLetters]
}

function bindEvents() {
    let allLetters = []
    for (let i = 0; i < squares.length; i++) {
        for (let f = 0; f < squares[0].length; f++) {
            const letter = squares[i][f].letter
            if (letter) {
                allLetters.push(letter)
            }
        }
    }

    allLetters = allLetters.concat(storage.letters)
    for (const letter of allLetters) {
        letter.bindEvents();
    }
}

var storage = {}

function makeStorage() {
    storage = new Storage()
    return storage.element;
}

function initializeWordMixFromStorage(availableLettersString) {
    initializeWordMix(getFromStorage(), availableLettersString)
}

function validateConnection() {
    function stringIt(point) {
        return point.x + ',' + point.y
    }

    const board = squares
    const allLetters = {}
    const visited = {}
    const queue = new Queue()
    let startPoint = undefined
    for (let x = 0; x < board.length; x++) {
        for (let y = 0; y < board[0].length; y++) {
            if (board[x][y].letter) {
                startPoint = startPoint || { x: x, y:y }
                allLetters[x + ',' + y] = true
            }
        }
    }

    if (!startPoint) {
        return 'Brädan är tom.'
    }

    function tryEnqueue(x, y) {
        if (x < 0 || y < 0 || x >= board.length || y >= board[0].length || visited[stringIt(({ x: x, y: y }))] || !board[x][y].letter) {
            return
        }

        queue.enqueue(({ x: x, y: y }))
    }

    queue.enqueue(startPoint)
    while (!queue.isEmpty()) {
        let point = queue.dequeue()
        visited[stringIt(point)] = true
        tryEnqueue(point.x - 1, point.y)
        tryEnqueue(point.x + 1, point.y)
        tryEnqueue(point.x, point.y - 1)
        tryEnqueue(point.x, point.y + 1)
    }

    for (const point in allLetters) {
        if (!visited[point]) {
            return 'Alla ord sitter inte ihop.'
        }
    }
}

function validateBoard() {
    let totalScore = 0
    const board = squares
    let connectionError = validateConnection()
    if (connectionError) {
        return connectionError
    }

    function validateSingle(elems, woord, points) {
        if (dict[woord.toLowerCase()]) {
            totalScore += points
            return true
        } else {
            for (const elem of elems) {
                elem.classList.add('word-mix-invalid')
            }

            return false
        }
    }

    function iterate(outerMax, innerMax, getLetter) {
        for (let outer = 0; outer < outerMax; outer++) {
            for (let inner = 0; inner < innerMax; inner++) {
                const letter = getLetter(outer, inner)
                if (letter) {
                    let elements = [letter.element]
                    let word = letter.letter
                    let score = letter.points
                    inner += 1
                    while (inner < innerMax && getLetter(outer, inner)) {
                        elements.push(getLetter(outer, inner).element)
                        word += getLetter(outer, inner).letter
                        score += getLetter(outer, inner).points
                        inner += 1
                    }

                    if (word.length > 1) {
                        if (!validateSingle(elements, word, score)) {
                            return word + ' är inte giltigt.'
                        }
                    }
                }
            }
        }
    }

    let error1 = iterate(board.length, board[0].length, (o, i) => board[o][i].letter)
    let error2 = iterate(board[0].length, board.length, (o, i) => board[i][o].letter)
    if (error1 || error2) {
        return error1 || error2
    }

    totalScore -= storage.letters.map(x => x.points).reduce((sum, val) => sum + val, 0)
    recordCallback(totalScore, BoardToString(board))
    return totalScore
}

function initializeWordMix(boardString, availableLettersString) {
    if (!boardString) {
        makeLettersForStorage(parseAvailableLetters(availableLettersString))
        return
    }

    function findAndRemove(item, items) {
        for (let i = 0; i < items.length; i++) {
            if (item.points === items[i].score && item.letter === items[i].letter) {
                items.splice(i, 1);
                return true
            }
        }

        return false
    }

    const board = StringToBoard(boardString)
    const boardLetters = board[0]
    const storageLetters = board[1]

    if (availableLettersString) {
        const availableLetters = parseAvailableLetters(availableLettersString)
        for (const letter of boardLetters.concat(storageLetters)) {
            if (!findAndRemove(letter, availableLetters)) {
                makeLettersForStorage(parseAvailableLetters(availableLettersString))
                return
            }
        }
    }

    for (const letter of boardLetters) {
        const elem = makeLetterElement(letter.points, letter.letter)
        letter.element = elem
        const rect = squares[letter.x][letter.y].element.getBoundingClientRect()
        elem.style.left = rect.left + 'px'
        elem.style.top = rect.top + 'px'
        squares[letter.x][letter.y].letter = letter
    }

    for (const letter of storageLetters) {
        const elem = makeLetterElement(letter.points, letter.letter)
        letter.element = elem
        storage.addLetter(letter)
    }

    storage.rePosition()
}

function makeLettersForStorage(lettersPairs) {
    for (const pair of lettersPairs) {
        makeLetterForStorage(pair.score, pair.letter)
    }

    storage.rePosition()
}

function makeLetterForStorage(score, letter) {
    const letterElement = makeLetterElement(score, letter)
    const letterObject = new Letter(null, null, score, letter, letterElement)
    storage.addLetter(letterObject)
}

function decodeForClient(string) {
    return string.replaceAll(".", "Ä").replaceAll("*", "Ö")
}

function parseAvailableLetters(availableLettersString) {
    const letterPairs = []
    availableLettersString = decodeForClient(availableLettersString)
    for (let i = 0; i < availableLettersString.length; i += 2) {
        const score = parseInt(availableLettersString[i + 1])
        letterPairs.push(({ letter: availableLettersString[i], score: score }))
    }

    return letterPairs
}

function makeLetterElement(score, letter) {
    const element = document.createElement('div');
    element.className = 'word-mix-letter-outer'
    element.style.width = squareSize + 'px'
    element.style.height = squareSize + 'px'

    const letterElement = document.createElement('span')
    letterElement.className = 'word-mix-letter';
    letterElement.innerText = letter;
    element.appendChild(letterElement)

    const number = document.createElement('span')
    number.className = 'word-mix-number';
    number.innerText = score.toString();
    element.appendChild(number)

    document.body.appendChild(element)
    return element
}