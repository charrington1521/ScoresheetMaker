const int WHITE_PLAYER_LED = 4;
const int BLACK_PLAYER_LED = 5;
const int WHITE_PLAYER_BUTTON = 12;
const int BLACK_PLAYER_BUTTON = 13;

const int N_PROMOTION_BUTTON = 8;
const int B_PROMOTION_BUTTON = 9;
const int R_PROMOTION_BUTTON = 10;
const int Q_PROMOTION_BUTTON = 11;


//Promotion Buttons... 

const int NUMBER_ROWS = 8;
const int NUMBER_COLS = 8;

int ROWS[NUMBER_ROWS] = {38, 40, 42, 44, 46, 48, 50, 52};
int COLS[NUMBER_COLS] = {37, 39, 41, 43, 45, 47, 49, 51};

bool sensorResults[NUMBER_ROWS][NUMBER_COLS];

const bool WHITE_PLAYER_TURN = true;
const bool BLACK_PLAYER_TURN = false;

const int QUIT_TIME_MS = 3000;

const int SYSTEM_DELAY_MS = 40;

//To start a chess clock, the black player presses there button, initiating the white players time

enum Button_state_t
{
  UP,
  FALL,
  DOWN,
  RISE
};

enum Button_result_t
{
  RELEASED,
  PRESSED,
  HOLD_UP,
  HOLD_DOWN,
  DEBOUNCING
};

enum System_state_t
{
  SCANNING,
  GAME_RESULT,
  DONE
};

Button_state_t whiteButtonState;
int whiteButtonTimer;

Button_state_t blackButtonState;
int blackButtonTimer;

bool bothClockButtonsDown = false;
int bothClockButtonsDownTime = 0;


Button_state_t nPromotionButtonState;
Button_state_t bPromotionButtonState;
Button_state_t rPromotionButtonState;
Button_state_t qPromotionButtonState;

int nPromotionTimer;
int bPromotionTimer;
int rPromotionTimer;
int qPromotionTimer;


bool playerTurn = BLACK_PLAYER_TURN;

System_state_t systemState = SCANNING;

/**
 * Setup the inputs of the system.
 */
void setupInputs();

/**
 * Setup the outputs of the system.
 */
void setupOutputs();

/**
 * Checks whether or not the chess clocks state should change. E.g. a player presses
 * the clock button during their turn, signalling that they have completed their move.
 * TO BE ADDED - allow game completion submission (via holding clock buttons?)
 */
void chessClockUpdate();

/**
 * Ends one player's turn and begins the others'. Expected to change the turn
 * as well as update the player LEDs. Additionally should set up and store relevant
 * board information for the turn ending and the one beginning. 
 */
void changeTurns();

/**
 * Changes the digital state of an output pin - A HIGH LED becomes LOW and a 
 * LOW LED becomes HIGH.
 */
void togglePin(const int pin);

void debounceButtonInit(const int pin);

void setup() {
  Serial.begin(9600);
  setupInputs();
  setupOutputs();
}

void loop() {
  switch (systemState)
  {
    case SCANNING:
      chessClockUpdate();
      promotionButtonUpdate();
      scanSensors();
      sendSensorResults();
      delay(SYSTEM_DELAY_MS-40);
    break;
    case GAME_RESULT:
      digitalWrite(WHITE_PLAYER_LED, LOW);
      digitalWrite(BLACK_PLAYER_LED, LOW);
      Button_result_t whiteButtonResult = debounceButtonUpdate(WHITE_PLAYER_BUTTON, &whiteButtonState, &whiteButtonTimer);
      Button_result_t blackButtonResult = debounceButtonUpdate(BLACK_PLAYER_BUTTON, &blackButtonState, &blackButtonTimer);
      if (bothButtonsDownUpdate(whiteButtonResult, blackButtonResult))
      {
        digitalWrite(WHITE_PLAYER_LED, HIGH);
        digitalWrite(BLACK_PLAYER_LED, HIGH);
        Serial.println("Game Result 1/2-1/2");
        systemState = DONE;
      }
      else if (whiteButtonResult == RELEASED)
      {
        digitalWrite(WHITE_PLAYER_LED, HIGH);
        Serial.println("Game Result 1-0");
        systemState = DONE;
      }
      else if (blackButtonResult == RELEASED)
      {
        digitalWrite(BLACK_PLAYER_LED, HIGH);
        Serial.println("Game Result 0-1");
        systemState = DONE;
      }
      delay(SYSTEM_DELAY_MS);
    break;
    default:
      delay(SYSTEM_DELAY_MS);
    break;
  }
}

void setupInputs() {
  debounceButtonInit(WHITE_PLAYER_BUTTON, &whiteButtonState, &whiteButtonTimer);
  debounceButtonInit(BLACK_PLAYER_BUTTON, &blackButtonState, &blackButtonTimer);

  debounceButtonInit(N_PROMOTION_BUTTON, &nPromotionButtonState, &nPromotionTimer);
  debounceButtonInit(B_PROMOTION_BUTTON, &bPromotionButtonState, &bPromotionTimer);
  debounceButtonInit(R_PROMOTION_BUTTON, &rPromotionButtonState, &rPromotionTimer);
  debounceButtonInit(Q_PROMOTION_BUTTON, &qPromotionButtonState, &qPromotionTimer);


  Serial.print("\n");
  for (int col = 0; col < NUMBER_COLS; col++) {
    pinMode(COLS[col], INPUT_PULLUP);
  }

  pinMode(2, INPUT_PULLUP);
  pinMode(3, INPUT_PULLUP);
}

void setupOutputs() {
  pinMode(WHITE_PLAYER_LED, OUTPUT);
  pinMode(BLACK_PLAYER_LED, OUTPUT);

  for (int row = 0; row < NUMBER_ROWS; row++) {
    pinMode(ROWS[row], OUTPUT);
  }

  digitalWrite(BLACK_PLAYER_LED, HIGH);
}

void debounceButtonInit(const int pin, Button_state_t * stateVar, int * timer)
{
  pinMode(pin, INPUT_PULLUP);
  if (digitalRead(pin) == LOW) 
  {
    *stateVar = DOWN;
  }
  else
  {
    *stateVar = UP;
  }

  *timer = 0;
}

void chessClockUpdate() {
  bool playerTurn = getPlayerTurn();
  
  Button_result_t whiteButtonResult = debounceButtonUpdate(WHITE_PLAYER_BUTTON, &whiteButtonState, &whiteButtonTimer);
  Button_result_t blackButtonResult = debounceButtonUpdate(BLACK_PLAYER_BUTTON, &blackButtonState, &blackButtonTimer);

  if (bothButtonsDownUpdate(whiteButtonResult, blackButtonResult))
  {
    Serial.println("quit");
    systemState = GAME_RESULT;
  }
  else
  {
    if (playerTurn == WHITE_PLAYER_TURN && whiteButtonResult == RELEASED
        || playerTurn == BLACK_PLAYER_TURN && blackButtonResult == RELEASED) {
      changeTurns();
    }
  }
}

bool bothButtonsDownUpdate(Button_result_t whiteButtonResult, Button_result_t blackButtonResult)
{
  if (whiteButtonResult == HOLD_DOWN && blackButtonResult == HOLD_DOWN)
  {
    if (! bothClockButtonsDown)
    {
      bothClockButtonsDownTime = 0;
      bothClockButtonsDown = true;
    }
    if (bothClockButtonsDownTime >= QUIT_TIME_MS)
    {
      bothClockButtonsDown = false;
      return true;
    }
    bothClockButtonsDownTime += SYSTEM_DELAY_MS;
  }
  else
  {
    bothClockButtonsDown = false;
  }
  return false;
}

void promotionButtonUpdate()
{
  if (debounceButtonUpdate(N_PROMOTION_BUTTON, &nPromotionButtonState, &nPromotionTimer) == RELEASED)
  {
    Serial.println("Promotion N");
  }
  if (debounceButtonUpdate(B_PROMOTION_BUTTON, &nPromotionButtonState, &bPromotionTimer) == RELEASED)
  {
    Serial.println("Promotion B");
  }
  if (debounceButtonUpdate(R_PROMOTION_BUTTON, &nPromotionButtonState, &rPromotionTimer) == RELEASED)
  {
    Serial.println("Promotion R");
  }
  if (debounceButtonUpdate(Q_PROMOTION_BUTTON, &nPromotionButtonState, &qPromotionTimer) == RELEASED)
  {
    Serial.println("Promotion Q");
  }
}

void changeTurns() {
  Serial.print("Turn changed\n");
  togglePin(WHITE_PLAYER_LED);
  togglePin(BLACK_PLAYER_LED);
  playerTurn = ! playerTurn;
  scanSensors();
  sendSensorResults();
  //Send turn changed. . .
}

bool getPlayerTurn() {
  return playerTurn;
}

void scanSensors() {
  for (int row = 0; row < NUMBER_ROWS; row++) {
    digitalWrite(ROWS[row], HIGH);
    delayMicroseconds(3);
    for (int col = 0; col < NUMBER_COLS; col++) {
      sensorResults[row][col] = (digitalRead(COLS[col]) == LOW);
    }
    digitalWrite(ROWS[row], LOW);
    delayMicroseconds(3);
  }
}

void sendSensorResults() {
  const char str[NUMBER_ROWS * (NUMBER_COLS + 1) + 2] = "";
  for (int row = 0; row < NUMBER_ROWS; row++) {
    for (int col = 0; col < NUMBER_COLS; col++) {
      if (sensorResults[row][col]) {
        strcat(str, "1");
      } else {
        strcat(str, "0");
      }
    }
    strcat(str, "\n");  
  }
  Serial.print(str);
  Serial.print(" \n");
}

void togglePin(int pin) {
  if (digitalRead(pin) == LOW) {
    digitalWrite(pin, HIGH);
  } else {
    digitalWrite(pin, LOW);
  }
}

Button_result_t debounceButtonUpdate(const int pin, Button_state_t * stateVar, int * timer)
{
  const static int DEBOUNCE_TIME_MS = 40;
  Button_result_t toReturn;
  int debounceTime = *timer;
  switch(*stateVar)
  {
    case UP:
      if (digitalRead(pin) == LOW)
      {
        *stateVar = FALL;
        *timer = 0;
        toReturn = PRESSED;
      }
      else
      {
        toReturn = HOLD_UP;
      }
    break;
    case FALL:
      if (debounceTime >= DEBOUNCE_TIME_MS)
      {
        if (digitalRead(pin) == LOW)
        {
          *stateVar = DOWN;
        }
        else
        {
          *stateVar = UP;
        }
      }
      *timer += SYSTEM_DELAY_MS;
      toReturn = DEBOUNCING;
    break;
    case DOWN:
      if (digitalRead(pin) == HIGH)
      {
        *stateVar = RISE;
        *timer = 0;
        toReturn = RELEASED;
      } 
      else
      {
        toReturn = HOLD_DOWN;
      }
    break;
    case RISE:
      if (debounceTime >= DEBOUNCE_TIME_MS)
      {
        if (digitalRead(pin) == LOW)
        {
          *stateVar = DOWN;
        }
        else
        {
          *stateVar = UP;
        }
      }
      *timer += SYSTEM_DELAY_MS;
      toReturn = DEBOUNCING;
    break;
    default:
      debounceButtonInit(pin, stateVar, timer);
      toReturn = DEBOUNCING;
    break;  
  }
  return toReturn;
}

void printButtonState(Button_state_t buttonState)
{
  switch(buttonState)
  {
    case UP:
      Serial.println("Up");
    break;
    case DOWN:
      Serial.println("Down");
    break;
    case FALL:
      Serial.println("Fall");
    break;
    case RISE:
      Serial.println("Rise");
    break;
    case DEBOUNCING:
      Serial.println("Busy");
    break;
    default:
      Serial.println("WTF");
    break;
  }
}

void printButtonResult(Button_result_t buttonResult)
{
  switch(buttonResult)
  {
    case HOLD_UP:
      Serial.println("Up. . .");
    break;
    case HOLD_DOWN:
      Serial.println("Holding down");
    break;
    case PRESSED:
      Serial.println("Pressed");
    break;
    case RELEASED:
      Serial.println("Released");
    break;
    case DEBOUNCING:
      Serial.println("Busy");
    break;
    default:
      Serial.println("WTF");
    break;
  }
}
